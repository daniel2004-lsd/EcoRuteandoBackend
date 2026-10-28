using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EcoRuteando.IntegrationTests.Infrastructure;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using EcoRuteando.Modules.Security.Presentation.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoRuteando.IntegrationTests.Security;

public sealed class RefreshTokenCookieE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RefreshTokenCookieE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(User User, string RawPassword)> CreateActiveUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var rawPassword = "CookieTestPassword123!";
        var email = $"cookie_{Guid.NewGuid():N}@test.ecoruteando.com";

        var user = new User(
            firstName: "Cookie",
            lastName: "Tester",
            email: email,
            passwordHash: passwordHasher.Hash(rawPassword),
            phoneNumber: "+573001122334");

        user.VerifyEmail();

        var regularRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "user");
        if (regularRole is null)
        {
            regularRole = new Role("user", "Regular User");
            context.Roles.Add(regularRole);
            await context.SaveChangesAsync();
        }

        user.AssignPrimaryRole(regularRole);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole(user.Id, regularRole.Id));
        await context.SaveChangesAsync();

        return (user, rawPassword);
    }

    private static (string CookieValue, string FullSetCookie) ExtractCookie(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
        {
            return (string.Empty, string.Empty);
        }

        foreach (var header in cookieHeaders)
        {
            if (header.StartsWith($"{cookieName}=", StringComparison.OrdinalIgnoreCase))
            {
                var cookiePart = header.Split(';')[0];
                var val = cookiePart.Substring(cookieName.Length + 1);
                return (val, header);
            }
        }

        return (string.Empty, string.Empty);
    }

    [Fact]
    public async Task WebLogin_SetsHttpOnlyCookie_WithExpectedSecurityAttributes_AndDoesNotExposeRefreshTokenInJson()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest(user.Email, password, null))
        };
        request.Headers.Add("X-Client-Platform", "web");
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // JSON check: accessToken present, refreshToken absent
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(body.TryGetProperty("accessToken", out var accessProp));
        Assert.False(string.IsNullOrWhiteSpace(accessProp.GetString()));

        if (body.TryGetProperty("refreshToken", out var refreshProp))
        {
            Assert.True(string.IsNullOrEmpty(refreshProp.GetString()), "refreshToken must NOT be exposed in JSON to web client");
        }

        // Cookie check
        var (cookieVal, fullHeader) = ExtractCookie(response, "refreshToken");
        Assert.False(string.IsNullOrWhiteSpace(cookieVal), "refreshToken cookie must be set");
        var headerLower = fullHeader.ToLowerInvariant();
        Assert.Contains("httponly", headerLower);
        Assert.Contains("samesite=strict", headerLower);
        Assert.Contains("path=/api/auth", headerLower);
    }

    [Fact]
    public async Task MobileLogin_PreservesContract_ReturnsRefreshTokenInJson()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest(user.Email, password, null))
        };
        // No web headers: standard mobile client request

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.True(body.TryGetProperty("accessToken", out var accessProp));
        Assert.False(string.IsNullOrWhiteSpace(accessProp.GetString()));

        Assert.True(body.TryGetProperty("refreshToken", out var refreshProp));
        Assert.False(string.IsNullOrWhiteSpace(refreshProp.GetString()), "Mobile contract must receive refreshToken in JSON");
    }

    [Fact]
    public async Task Refresh_WithValidCookie_AndAuthorizedOrigin_RotatesCookie_AndReturnsNewAccessToken()
    {
        // Arrange: login as web client
        var (user, password) = await CreateActiveUserAsync();

        var loginReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest(user.Email, password, null))
        };
        loginReq.Headers.Add("X-Client-Platform", "web");
        loginReq.Headers.Add("Origin", "http://localhost:3000");

        var loginRes = await _client.SendAsync(loginReq);
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

        var (initialCookie, _) = ExtractCookie(loginRes, "refreshToken");
        Assert.False(string.IsNullOrWhiteSpace(initialCookie));

        // Act: Refresh using cookie
        var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new { })
        };
        refreshReq.Headers.Add("Cookie", $"refreshToken={initialCookie}");
        refreshReq.Headers.Add("Origin", "http://localhost:3000");
        refreshReq.Headers.Add("X-Client-Platform", "web");

        var refreshRes = await _client.SendAsync(refreshReq);

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshRes.StatusCode);

        var body = await refreshRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(body.TryGetProperty("accessToken", out var newAccessProp));
        Assert.False(string.IsNullOrWhiteSpace(newAccessProp.GetString()));

        if (body.TryGetProperty("refreshToken", out var refreshProp))
        {
            Assert.True(string.IsNullOrEmpty(refreshProp.GetString()));
        }

        var (rotatedCookie, fullRotatedHeader) = ExtractCookie(refreshRes, "refreshToken");
        Assert.False(string.IsNullOrWhiteSpace(rotatedCookie));
        Assert.NotEqual(initialCookie, rotatedCookie); // Token rotation
        Assert.Contains("httponly", fullRotatedHeader.ToLowerInvariant());
        Assert.Contains("samesite=strict", fullRotatedHeader.ToLowerInvariant());
    }

    [Fact]
    public async Task Refresh_WithoutCookie_AndWithoutBody_ReturnsUnauthorized()
    {
        // Act: Refresh with empty body and no cookies
        var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new { })
        };

        var response = await _client.SendAsync(refreshReq);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithCookie_FromUnauthorizedOrigin_IsRejectedWithForbidden()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest(user.Email, password, null))
        };
        loginReq.Headers.Add("X-Client-Platform", "web");
        loginReq.Headers.Add("Origin", "http://localhost:3000");

        var loginRes = await _client.SendAsync(loginReq);
        var (cookieVal, _) = ExtractCookie(loginRes, "refreshToken");

        // Act: Attempt refresh from unauthorized attacker origin
        var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new { })
        };
        refreshReq.Headers.Add("Cookie", $"refreshToken={cookieVal}");
        refreshReq.Headers.Add("Origin", "https://malicious-attacker.com");
        refreshReq.Headers.Add("X-Client-Platform", "web");

        var response = await _client.SendAsync(refreshReq);

        // Assert: CSRF origin check rejects request
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithCookie_MissingAntiCsrfHeader_IsRejectedWithForbidden()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest(user.Email, password, null))
        };
        loginReq.Headers.Add("X-Client-Platform", "web");
        loginReq.Headers.Add("Origin", "http://localhost:3000");

        var loginRes = await _client.SendAsync(loginReq);
        var (cookieVal, _) = ExtractCookie(loginRes, "refreshToken");

        // Act: Attempt refresh without X-Client-Platform or anti-CSRF header
        var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
        {
            Content = JsonContent.Create(new { })
        };
        refreshReq.Headers.Add("Cookie", $"refreshToken={cookieVal}");
        refreshReq.Headers.Add("Origin", "http://localhost:3000");
        // Omit X-Client-Platform and X-Requested-With

        var response = await _client.SendAsync(refreshReq);

        // Assert: Missing anti-CSRF header rejects request
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithCookie_DeletesCookie_AndRevokesSession()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest(user.Email, password, null))
        };
        loginReq.Headers.Add("X-Client-Platform", "web");
        loginReq.Headers.Add("Origin", "http://localhost:3000");

        var loginRes = await _client.SendAsync(loginReq);
        var loginBody = await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = loginBody.GetProperty("accessToken").GetString();
        var (cookieVal, _) = ExtractCookie(loginRes, "refreshToken");

        // Verify access token is active before logout
        var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meRes = await _client.SendAsync(meReq);
        Assert.Equal(HttpStatusCode.OK, meRes.StatusCode);

        // Act: Logout sending cookie
        var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = JsonContent.Create(new { })
        };
        logoutReq.Headers.Add("Cookie", $"refreshToken={cookieVal}");

        var logoutRes = await _client.SendAsync(logoutReq);
        Assert.Equal(HttpStatusCode.NoContent, logoutRes.StatusCode);

        // Assert: Cookie deletion was commanded in Set-Cookie header
        var (_, deleteCookieHeader) = ExtractCookie(logoutRes, "refreshToken");
        var lowerDelete = deleteCookieHeader.ToLowerInvariant();
        Assert.True(lowerDelete.Contains("expires=") || lowerDelete.Contains("max-age=0"), "Cookie must be expired on logout");

        // Assert: Session was revoked in DB, access token is now invalid
        var postLogoutReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        postLogoutReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var postLogoutRes = await _client.SendAsync(postLogoutReq);
        Assert.Equal(HttpStatusCode.Unauthorized, postLogoutRes.StatusCode);
    }

    [Fact]
    public async Task MobileRefresh_PreservesContract_WorksWithBodyRefreshToken()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var initialRefreshToken = loginBody.GetProperty("refreshToken").GetString();

        // Act: Mobile refresh sends refreshToken in JSON body without cookies
        var refreshRes = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(initialRefreshToken!));

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshRes.StatusCode);
        var refreshBody = await refreshRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.True(refreshBody.TryGetProperty("accessToken", out var accessProp));
        Assert.False(string.IsNullOrWhiteSpace(accessProp.GetString()));

        Assert.True(refreshBody.TryGetProperty("refreshToken", out var refreshProp));
        Assert.False(string.IsNullOrWhiteSpace(refreshProp.GetString()));
        Assert.NotEqual(initialRefreshToken, refreshProp.GetString());
    }
}
