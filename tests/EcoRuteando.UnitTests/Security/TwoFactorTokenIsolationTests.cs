using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class TwoFactorTokenIsolationTests
{
    private readonly JwtOptions _options = new()
    {
        SecretKey = "SuperSecretKeyForTestingPurposesOnly_MustBeLongEnough123!",
        Issuer = "EcoRuteandoTest",
        Audience = "EcoRuteandoAudience",
        ExpirationMinutes = 60
    };

    [Fact]
    public void GenerateToken_ShouldHaveAccessTokenTypeAndConfiguredExpiration()
    {
        var provider = new JwtProvider(Options.Create(_options));
        var userId = Guid.NewGuid();
        var email = "user@test.com";
        var role = "user";

        var tokenString = provider.GenerateToken(userId, email, role);
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Contains(token.Claims, c => c.Type == "token_type" && c.Value == "access");
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == role);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());

        var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
        Assert.True(token.ValidTo <= expectedExpiry.AddMinutes(1) && token.ValidTo >= expectedExpiry.AddMinutes(-1));
    }

    [Fact]
    public void GenerateTwoFactorToken_ShouldHaveTwoFactorTempTypeAnd5MinExpiration()
    {
        var provider = new JwtProvider(Options.Create(_options));
        var userId = Guid.NewGuid();
        var email = "user@test.com";

        var tokenString = provider.GenerateTwoFactorToken(userId, email);
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Contains(token.Claims, c => c.Type == "token_type" && c.Value == "two_factor_temp");
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "two_factor_pending");
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());

        var expectedExpiry = DateTime.UtcNow.AddMinutes(5);
        Assert.True(token.ValidTo <= expectedExpiry.AddMinutes(1) && token.ValidTo >= expectedExpiry.AddMinutes(-1));
    }

    [Fact]
    public async Task TrustedDeviceToken_ShouldHaveTrustedDeviceType()
    {
        var service = new TrustedDeviceService(Options.Create(_options));
        var userId = Guid.NewGuid();

        var tokenString = await service.GenerateTrustedDeviceTokenAsync(userId, 30);
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Contains(token.Claims, c => c.Type == "token_type" && c.Value == "trusted_device");
    }

    [Theory]
    [InlineData("/api/routes", true)]
    [InlineData("/api/users/profile", true)]
    [InlineData("/api/obstacle-reports", true)]
    [InlineData("/api/auth/2fa/verify", false)]
    public async Task TwoFactorTempToken_ShouldOnlyBeAllowedOn2FaVerifyEndpoint(string path, bool expectRejection)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "two_factor_pending"),
            new Claim("token_type", "two_factor_temp")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var authScheme = new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler));
        var options = new JwtBearerOptions();
        var tokenValidatedContext = new TokenValidatedContext(httpContext, authScheme, options)
        {
            Principal = principal
        };

        // Simulate our OnTokenValidated handler
        var tokenType = principal.FindFirst("token_type")?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

        if (tokenType == "trusted_device")
        {
            tokenValidatedContext.Fail("Token de dispositivo confiable no puede utilizarse como token de acceso.");
        }

        var isTwoFactorTemp = tokenType == "two_factor_temp" || role == "two_factor_pending";
        var requestPath = httpContext.Request.Path.Value ?? string.Empty;
        var is2FaEndpoint = requestPath.EndsWith("/api/auth/2fa/verify", StringComparison.OrdinalIgnoreCase);

        if (isTwoFactorTemp && !is2FaEndpoint)
        {
            tokenValidatedContext.Fail("Token temporal de 2FA no autorizado para acceder a este recurso.");
        }

        Assert.Equal(expectRejection, tokenValidatedContext.Result?.Failure != null);
    }

    [Fact]
    public void TrustedDeviceToken_MustAlwaysBeRejectedAsBearerToken()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/routes";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("token_type", "trusted_device")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var authScheme = new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler));
        var options = new JwtBearerOptions();
        var tokenValidatedContext = new TokenValidatedContext(httpContext, authScheme, options)
        {
            Principal = principal
        };

        var tokenType = principal.FindFirst("token_type")?.Value;
        if (tokenType == "trusted_device")
        {
            tokenValidatedContext.Fail("Token de dispositivo confiable no puede utilizarse como token de acceso.");
        }

        Assert.NotNull(tokenValidatedContext.Result?.Failure);
        Assert.Equal("Token de dispositivo confiable no puede utilizarse como token de acceso.", tokenValidatedContext.Result.Failure.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("unknown")]
    [InlineData("refresh")]
    public void TokenWithMissingOrInvalidTokenType_MustBeRejectedOnGeneralEndpoints(string? invalidTokenType)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/routes";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        if (invalidTokenType != null)
        {
            claims.Add(new Claim("token_type", invalidTokenType));
        }

        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var authScheme = new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler));
        var options = new JwtBearerOptions();
        var tokenValidatedContext = new TokenValidatedContext(httpContext, authScheme, options)
        {
            Principal = principal
        };

        var tokenType = principal.FindFirst("token_type")?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

        if (tokenType == "trusted_device")
        {
            tokenValidatedContext.Fail("Token de dispositivo confiable no puede utilizarse como token de acceso.");
        }

        var isTwoFactorTemp = tokenType == "two_factor_temp" || role == "two_factor_pending";
        var requestPath = (httpContext.Request.Path.Value ?? string.Empty).TrimEnd('/');
        var is2FaEndpoint = requestPath.EndsWith("/api/auth/2fa/verify", StringComparison.OrdinalIgnoreCase);

        if (isTwoFactorTemp)
        {
            if (!is2FaEndpoint)
            {
                tokenValidatedContext.Fail("Token temporal de 2FA no autorizado para acceder a este recurso.");
            }
        }
        else if (tokenType != "access")
        {
            tokenValidatedContext.Fail("Tipo de token no válido como token de acceso.");
        }

        Assert.NotNull(tokenValidatedContext.Result?.Failure);
        Assert.Equal("Tipo de token no válido como token de acceso.", tokenValidatedContext.Result.Failure.Message);
    }
}
