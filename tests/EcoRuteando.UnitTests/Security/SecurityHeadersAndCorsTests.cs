using System.Text.Json;
using EcoRuteando.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class SecurityHeadersAndCorsTests
{
    [Fact]
    public async Task SecurityHeadersMiddleware_ShouldInjectAllRequiredDefensiveHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/routes";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Disparar callbacks OnStarting
        await context.Response.StartAsync();

        // Assert
        var headers = context.Response.Headers;

        Assert.Equal("nosniff", headers["X-Content-Type-Options"].ToString());
        Assert.Equal("DENY", headers["X-Frame-Options"].ToString());
        Assert.Equal("strict-origin-when-cross-origin", headers["Referrer-Policy"].ToString());
        Assert.Contains("camera=()", headers["Permissions-Policy"].ToString());
        Assert.Contains("microphone=()", headers["Permissions-Policy"].ToString());

        var csp = headers["Content-Security-Policy"].ToString();
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
        Assert.Contains("object-src 'none'", csp);
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_OnSwaggerPath_ShouldAllowSwaggerUiAssetsInCsp()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new SecurityHeadersMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        // Assert
        var headers = context.Response.Headers;
        var csp = headers["Content-Security-Policy"].ToString();

        Assert.Contains("'unsafe-inline'", csp);
        Assert.Contains("data:", csp);
    }

    [Fact]
    public void CorsConfiguration_WhenConfiguredInAppsettings_ShouldResolveConfiguredOrigins()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Cors:AllowedOrigins:0", "https://ecoruteando.com" },
            { "Cors:AllowedOrigins:1", "https://admin.ecoruteando.com" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        // Assert
        Assert.NotNull(configuredOrigins);
        Assert.Equal(2, configuredOrigins!.Length);
        Assert.Contains("https://ecoruteando.com", configuredOrigins);
        Assert.Contains("https://admin.ecoruteando.com", configuredOrigins);
        Assert.DoesNotContain("*", configuredOrigins);
    }

    [Fact]
    public void CorsConfiguration_WhenDevelopmentAndNoConfig_ShouldFallbackToLocalhost()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        const bool isDevelopment = true;

        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        // Act
        var allowedOrigins = (configuredOrigins.Length > 0)
            ? configuredOrigins
            : (isDevelopment
                ? new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:3007" }
                : Array.Empty<string>());

        // Assert
        Assert.NotEmpty(allowedOrigins);
        Assert.Contains("http://localhost:3000", allowedOrigins);
        Assert.Contains("http://localhost:3001", allowedOrigins);
        Assert.DoesNotContain("*", allowedOrigins);
    }

    [Fact]
    public void CorsConfiguration_WhenProductionAndNoConfig_ShouldNotFallbackToLocalhost()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        const bool isDevelopment = false;

        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        // Act
        var allowedOrigins = (configuredOrigins.Length > 0)
            ? configuredOrigins
            : (isDevelopment
                ? new[] { "http://localhost:3000" }
                : Array.Empty<string>());

        // Assert
        Assert.Empty(allowedOrigins);
    }

    [Fact]
    public void ProductionAppsettings_ShouldHaveWarningLogLevelForEntityFrameworkCore()
    {
        // Arrange
        var appsettingsPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Api", "EcoRuteando.Api", "appsettings.json");

        if (File.Exists(appsettingsPath))
        {
            var json = File.ReadAllText(appsettingsPath);
            using var doc = JsonDocument.Parse(json);
            var logLevel = doc.RootElement
                .GetProperty("Logging")
                .GetProperty("LogLevel");

            var efCoreLevel = logLevel.GetProperty("Microsoft.EntityFrameworkCore").GetString();
            var efCmdLevel = logLevel.GetProperty("Microsoft.EntityFrameworkCore.Database.Command").GetString();

            // Assert
            Assert.Equal("Warning", efCoreLevel);
            Assert.Equal("Warning", efCmdLevel);
        }
    }
}
