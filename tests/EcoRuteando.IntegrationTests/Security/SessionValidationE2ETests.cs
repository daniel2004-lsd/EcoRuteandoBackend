using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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

public sealed class SessionValidationE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SessionValidationE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(User User, string RawPassword)> CreateActiveUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var rawPassword = "SessionTestPassword123!";
        var email = $"session_{Guid.NewGuid():N}@test.ecoruteando.com";

        var user = new User(
            firstName: "Session",
            lastName: "Tester",
            email: email,
            passwordHash: passwordHasher.Hash(rawPassword),
            phoneNumber: "+573009998877");

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

    [Fact]
    public async Task Scenario1_ActiveSession_AccessToken_SucceedsOnProtectedEndpoint()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        // Step 1: Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = loginBody.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        // Act: GET /api/auth/me with active access token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);

        // Assert: Access is granted
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var meBody = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(user.Email, meBody.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Scenario2_Logout_ImmediatelyRevokesSession_AndAccessTokenIsRejected()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = loginBody.GetProperty("accessToken").GetString();
        var refreshToken = loginBody.GetProperty("refreshToken").GetString();

        // Verify active token works before logout
        var preLogoutReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        preLogoutReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var preLogoutRes = await _client.SendAsync(preLogoutReq);
        Assert.Equal(HttpStatusCode.OK, preLogoutRes.StatusCode);

        // Act: Logout using refresh token
        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(refreshToken!));
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        // Assert: Subsequent request using previous accessToken must be rejected with HTTP 401
        var postLogoutReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        postLogoutReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var postLogoutRes = await _client.SendAsync(postLogoutReq);

        Assert.Equal(HttpStatusCode.Unauthorized, postLogoutRes.StatusCode);
    }

    [Fact]
    public async Task Scenario3_RevokeSingleSession_ImmediatelyInvalidatesItsAccessToken()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = loginBody.GetProperty("accessToken").GetString();

        // Retrieve session ID via GET /api/auth/sessions
        var getSessionsReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/sessions");
        getSessionsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var getSessionsRes = await _client.SendAsync(getSessionsReq);
        Assert.Equal(HttpStatusCode.OK, getSessionsRes.StatusCode);

        var sessions = await getSessionsRes.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOptions);
        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);
        var sessionId = sessions[0].GetProperty("id").GetGuid();

        // Act: DELETE /api/auth/sessions/{sessionId}
        var deleteSessionReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/auth/sessions/{sessionId}");
        deleteSessionReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteSessionRes = await _client.SendAsync(deleteSessionReq);
        Assert.Equal(HttpStatusCode.NoContent, deleteSessionRes.StatusCode);

        // Assert: Immediately rejected on subsequent request
        var postRevokeReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        postRevokeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var postRevokeRes = await _client.SendAsync(postRevokeReq);

        Assert.Equal(HttpStatusCode.Unauthorized, postRevokeRes.StatusCode);
    }

    [Fact]
    public async Task Scenario4_RevokeAllSessions_ImmediatelyInvalidatesAllAccessTokens()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = loginBody.GetProperty("accessToken").GetString();

        // Act: DELETE /api/auth/sessions (revoke all)
        var revokeAllReq = new HttpRequestMessage(HttpMethod.Delete, "/api/auth/sessions");
        revokeAllReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var revokeAllRes = await _client.SendAsync(revokeAllReq);
        Assert.Equal(HttpStatusCode.OK, revokeAllRes.StatusCode);

        // Assert: Immediately rejected on subsequent request
        var postRevokeAllReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        postRevokeAllReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var postRevokeAllRes = await _client.SendAsync(postRevokeAllReq);

        Assert.Equal(HttpStatusCode.Unauthorized, postRevokeAllRes.StatusCode);
    }

    [Fact]
    public async Task Scenario5_ChangePassword_ImmediatelyInvalidatesPreviousAccessTokens()
    {
        // Arrange
        var (user, oldPassword) = await CreateActiveUserAsync();
        const string newPassword = "NewValidSecurePassword123!";

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, oldPassword, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var oldAccessToken = loginBody.GetProperty("accessToken").GetString();

        // Act: Change password via POST /api/auth/change-password
        var changePwReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-password")
        {
            Content = JsonContent.Create(new ChangeMyPasswordRequest(oldPassword, newPassword))
        };
        changePwReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oldAccessToken);
        var changePwRes = await _client.SendAsync(changePwReq);
        Assert.Equal(HttpStatusCode.OK, changePwRes.StatusCode);

        // Assert 1: Old accessToken is rejected immediately with 401
        var postChangeReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        postChangeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oldAccessToken);
        var postChangeRes = await _client.SendAsync(postChangeReq);
        Assert.Equal(HttpStatusCode.Unauthorized, postChangeRes.StatusCode);

        // Assert 2: New login with new password succeeds and works
        var newLoginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, newPassword, null));
        Assert.Equal(HttpStatusCode.OK, newLoginRes.StatusCode);
        var newLoginBody = await newLoginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var newAccessToken = newLoginBody.GetProperty("accessToken").GetString();

        var newMeReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        newMeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
        var newMeRes = await _client.SendAsync(newMeReq);
        Assert.Equal(HttpStatusCode.OK, newMeRes.StatusCode);
    }

    [Fact]
    public async Task Scenario6_ResetPassword_ImmediatelyInvalidatesPreviousAccessTokens()
    {
        // Arrange
        var (user, oldPassword) = await CreateActiveUserAsync();
        const string newPassword = "ResetSecurePassword999!";

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, oldPassword, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var oldAccessToken = loginBody.GetProperty("accessToken").GetString();

        // Generate unique recovery token in database
        var rawRecoveryCode = $"REC_{Guid.NewGuid():N}";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRecoveryCode)));

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
            var recovery = new PasswordRecovery(user.Id, tokenHash, DateTime.UtcNow.AddHours(1), IPAddress.Loopback);
            context.PasswordRecoveries.Add(recovery);
            await context.SaveChangesAsync();
        }

        // Act: Reset password via POST /api/auth/reset-password
        var resetResponse = await _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest(
            rawRecoveryCode,
            newPassword,
            Email: user.Email));
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        // Assert: Old accessToken is rejected immediately with 401
        var postResetReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        postResetReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oldAccessToken);
        var postResetRes = await _client.SendAsync(postResetReq);
        Assert.Equal(HttpStatusCode.Unauthorized, postResetRes.StatusCode);
    }

    [Fact]
    public async Task Scenario7_RefreshToken_RevokesOldSession_OldAccessTokenIsRejected_NewAccessTokenWorks()
    {
        // Arrange
        var (user, password) = await CreateActiveUserAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken1 = loginBody.GetProperty("accessToken").GetString();
        var refreshToken1 = loginBody.GetProperty("refreshToken").GetString();

        // Act: Refresh tokens
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshToken1!));
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken2 = refreshBody.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken2));

        // Assert 1: Old accessToken1 is rejected with 401 because its session was revoked
        var oldTokenReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        oldTokenReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken1);
        var oldTokenRes = await _client.SendAsync(oldTokenReq);
        Assert.Equal(HttpStatusCode.Unauthorized, oldTokenRes.StatusCode);

        // Assert 2: New accessToken2 is accepted with 200 OK
        var newTokenReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        newTokenReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);
        var newTokenRes = await _client.SendAsync(newTokenReq);
        Assert.Equal(HttpStatusCode.OK, newTokenRes.StatusCode);
    }
}
