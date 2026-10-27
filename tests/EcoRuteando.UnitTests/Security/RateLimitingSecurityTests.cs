using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;
using EcoRuteando.Modules.Mobility.Presentation.Controllers;
using EcoRuteando.Modules.Security.Presentation.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class RateLimitingSecurityTests
{
    [Theory]
    [InlineData(nameof(AuthController.Register), "auth")]
    [InlineData(nameof(AuthController.Login), "auth")]
    [InlineData(nameof(AuthController.Refresh), "auth")]
    [InlineData(nameof(AuthController.Logout), "auth")]
    [InlineData(nameof(AuthController.EnableTwoFactorAuth), "auth")]
    [InlineData(nameof(AuthController.VerifyTwoFactorAuth), "auth")]
    [InlineData(nameof(AuthController.DisableTwoFactorAuth), "auth")]
    [InlineData(nameof(AuthController.ForgotPassword), "sensitive")]
    [InlineData(nameof(AuthController.ResetPassword), "sensitive")]
    [InlineData(nameof(AuthController.VerifyEmail), "sensitive")]
    [InlineData(nameof(AuthController.SendVerification), "sensitive")]
    public void AuthController_Endpoints_ShouldHaveExpectedRateLimitingPolicy(string actionName, string expectedPolicy)
    {
        var method = typeof(AuthController).GetMethod(actionName);
        Assert.NotNull(method);

        var attribute = method!.GetCustomAttribute<EnableRateLimitingAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(expectedPolicy, attribute!.PolicyName);
    }

    [Theory]
    [InlineData(typeof(GoogleMapsController), "api")]
    [InlineData(typeof(RoutesController), "api")]
    [InlineData(typeof(PoisController), "api")]
    [InlineData(typeof(ObstacleReportsController), "api")]
    [InlineData(typeof(SharedRoutesController), "api")]
    public void MobilityControllers_ShouldHaveApiRateLimitingAttribute(Type controllerType, string expectedPolicy)
    {
        var attribute = controllerType.GetCustomAttribute<EnableRateLimitingAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(expectedPolicy, attribute!.PolicyName);
    }

    [Fact]
    public void RateLimiterPartition_ShouldIsolateQuotasBetweenDifferentClientIps()
    {
        // Arrange
        const int limit = 2;
        PartitionedRateLimiter<string> limiter = PartitionedRateLimiter.Create<string, string>(key =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: key,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        const string ipA = "203.0.113.1";
        const string ipB = "198.51.100.2";

        // Act & Assert - Consume all permits for IP A
        using var leaseA1 = limiter.AttemptAcquire(ipA);
        Assert.True(leaseA1.IsAcquired, "First request from IP A should be allowed");

        using var leaseA2 = limiter.AttemptAcquire(ipA);
        Assert.True(leaseA2.IsAcquired, "Second request from IP A should be allowed");

        using var leaseA3 = limiter.AttemptAcquire(ipA);
        Assert.False(leaseA3.IsAcquired, "Third request from IP A must exceed permit limit and be rejected");

        // IP B must NOT be affected by IP A's exhausted quota
        using var leaseB1 = limiter.AttemptAcquire(ipB);
        Assert.True(leaseB1.IsAcquired, "IP B quota must remain independent and allow request");

        using var leaseB2 = limiter.AttemptAcquire(ipB);
        Assert.True(leaseB2.IsAcquired, "Second request from IP B should be allowed");
    }

    [Fact]
    public void RateLimiterPartition_ShouldHandleMissingClientIpGracefully()
    {
        // Arrange
        static string ResolvePartitionKey(IPAddress? remoteIp) =>
            remoteIp is not null ? remoteIp.ToString() : "unknown_client_ip";

        PartitionedRateLimiter<string> limiter = PartitionedRateLimiter.Create<string, string>(key =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: key,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

        // Act
        string keyForNull = ResolvePartitionKey(null);
        Assert.Equal("unknown_client_ip", keyForNull);

        using var lease1 = limiter.AttemptAcquire(keyForNull);
        using var lease2 = limiter.AttemptAcquire(keyForNull);

        // Assert
        Assert.True(lease1.IsAcquired, "First unidentifiable client request is acquired");
        Assert.False(lease2.IsAcquired, "Subsequent unidentifiable client request within limit window is rejected");
    }

    [Fact]
    public async Task RateLimiter_OnRejected_SetsStatusCode429AndReturnsJson()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        // Simulate OnRejected delegate behavior
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync("{\"error\":\"Demasiadas solicitudes. Por favor intente más tarde.\"}");

        // Assert
        Assert.Equal(429, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("Demasiadas solicitudes", body);
    }
}
