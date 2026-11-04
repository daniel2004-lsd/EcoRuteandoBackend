using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Storage;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetMyObstacleReports;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetObstacleReportsForValidation;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Presentation.Controllers;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility;

public sealed class ObstacleReportsAuthorizationTests
{
    private static async Task<WebApplication> CreateTestAppAsync(IMediator mediator)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services.AddSingleton(mediator);
        builder.Services.AddSingleton(new Mock<IPhotoStorageService>().Object);

        builder.Services
            .AddAuthentication("TestAuthScheme")
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>("TestAuthScheme", null);

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, HeaderPermissionHandler>();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(ObstacleReportsController).Assembly);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.StartAsync();
        return app;
    }

    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string path,
        Guid? userId = null,
        string? permissions = null)
    {
        var req = new HttpRequestMessage(method, path);
        if (userId.HasValue)
        {
            req.Headers.Add("X-Test-UserId", userId.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(permissions))
        {
            req.Headers.Add("X-Test-Permissions", permissions);
        }

        return req;
    }

    [Fact]
    public async Task ObstacleReportsEndpoints_AuthorizationMatrix_EnforcesStrictSecuritySeparation()
    {
        // Arrange Mediator mocks for GetMine and GetAll
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetMyObstacleReportsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ObstacleReportResponse>());

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetObstacleReportsForValidationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ObstacleReportResponse>());

        await using var app = await CreateTestAppAsync(mediatorMock.Object);
        var baseAddress = app.Urls.First();
        using var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

        var testUserId = Guid.NewGuid();

        // 1. Sin autenticación -> Ambos endpoints deben retornar HTTP 401 Unauthorized
        using (var reqGetAllUnauth = CreateRequest(HttpMethod.Get, "/api/obstacle-reports"))
        {
            var resp = await client.SendAsync(reqGetAllUnauth);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        using (var reqGetMineUnauth = CreateRequest(HttpMethod.Get, "/api/obstacle-reports/mine"))
        {
            var resp = await client.SendAsync(reqGetMineUnauth);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        // 2. Rol Ciudadano / User (solo tiene permiso 'reports.read')
        // GET /api/obstacle-reports (cola administrativa) -> DEBE RETORNAR 403 Forbidden
        // GET /api/obstacle-reports/mine (mis reportes) -> DEBE RETORNAR 200 OK
        using (var reqGetAllUser = CreateRequest(HttpMethod.Get, "/api/obstacle-reports", testUserId, "reports.read"))
        {
            var resp = await client.SendAsync(reqGetAllUser);
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        using (var reqGetMineUser = CreateRequest(HttpMethod.Get, "/api/obstacle-reports/mine", testUserId, "reports.read"))
        {
            var resp = await client.SendAsync(reqGetMineUser);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        // 3. Validador autorizado (tiene permiso 'reports.validate' y 'reports.read')
        // GET /api/obstacle-reports -> DEBE RETORNAR 200 OK
        // GET /api/obstacle-reports/mine -> DEBE RETORNAR 200 OK
        using (var reqGetAllValidator = CreateRequest(HttpMethod.Get, "/api/obstacle-reports", testUserId, "reports.validate,reports.read"))
        {
            var resp = await client.SendAsync(reqGetAllValidator);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        using (var reqGetMineValidator = CreateRequest(HttpMethod.Get, "/api/obstacle-reports/mine", testUserId, "reports.validate,reports.read"))
        {
            var resp = await client.SendAsync(reqGetMineValidator);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        // 4. Administrador autorizado (tiene permisos 'reports.validate', 'reports.read', 'reports.create', etc.)
        // GET /api/obstacle-reports -> DEBE RETORNAR 200 OK
        // GET /api/obstacle-reports/mine -> DEBE RETORNAR 200 OK
        using (var reqGetAllAdmin = CreateRequest(HttpMethod.Get, "/api/obstacle-reports", testUserId, "reports.validate,reports.read,reports.create,reports.vote"))
        {
            var resp = await client.SendAsync(reqGetAllAdmin);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        using (var reqGetMineAdmin = CreateRequest(HttpMethod.Get, "/api/obstacle-reports/mine", testUserId, "reports.validate,reports.read,reports.create,reports.vote"))
        {
            var resp = await client.SendAsync(reqGetMineAdmin);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        await app.StopAsync();
    }

    private sealed class HeaderAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public HeaderAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-UserId", out var userIdValues) ||
                string.IsNullOrWhiteSpace(userIdValues.FirstOrDefault()))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userIdValues.First()!)
            };

            if (Request.Headers.TryGetValue("X-Test-Permissions", out var permValues))
            {
                foreach (var perm in permValues.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    claims.Add(new Claim("permission", perm));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private sealed class HeaderPermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
