using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EcoRuteando.IntegrationTests.Infrastructure;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoRuteando.IntegrationTests.Security;

public sealed class TrustedDevicesAndTwoFactorE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TrustedDevicesAndTwoFactorE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string GenerateTotpCode(string secret)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var cleanInput = secret.TrimEnd('=').ToUpperInvariant();
        var byteCount = cleanInput.Length * 5 / 8;
        var secretBytes = new byte[byteCount];
        var buffer = 0;
        var bitsInBuffer = 0;
        var index = 0;

        foreach (var c in cleanInput)
        {
            var val = alphabet.IndexOf(c);
            if (val < 0) continue;
            buffer = (buffer << 5) | val;
            bitsInBuffer += 5;
            if (bitsInBuffer >= 8)
            {
                bitsInBuffer -= 8;
                if (index < secretBytes.Length) secretBytes[index++] = (byte)(buffer >> bitsInBuffer);
            }
        }

        var timeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian) Array.Reverse(timeBytes);

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);
        var offset = hash[^1] & 0x0F;
        var code = ((hash[offset] & 0x7F) << 24)
                 | ((hash[offset + 1] & 0xFF) << 16)
                 | ((hash[offset + 2] & 0xFF) << 8)
                 | (hash[offset + 3] & 0xFF);

        return (code % 1_000_000).ToString("D6");
    }

    private async Task<(User User, string RawPassword, string TotpSecret)> CreateUserWith2FAAsync(bool enable2Fa = true)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var totpService = scope.ServiceProvider.GetRequiredService<ITotpService>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

        var rawPassword = "TestPassword123!";
        var email = $"user_{Guid.NewGuid():N}@test.ecoruteando.com";

        var user = new User(
            firstName: "Integration",
            lastName: "Tester",
            email: email,
            passwordHash: passwordHasher.Hash(rawPassword),
            phoneNumber: "+573001234567");

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

        var secret = totpService.GenerateSecret();
        if (enable2Fa)
        {
            var encryptedSecret = encryptionService.Encrypt(Encoding.UTF8.GetBytes(secret));
            var twoFactor = new TwoFactorAuth(user.Id, TwoFactorMethod.TOTP, encryptedSecret);
            twoFactor.Enable();
            context.TwoFactorAuths.Add(twoFactor);
            await context.SaveChangesAsync();
        }

        return (user, rawPassword, secret);
    }

    [Fact]
    public async Task Scenario1_Login_WithValidCredentials_AndActive2FA_DemandsTwoFactorVerification()
    {
        // Arrange
        var (user, password, _) = await CreateUserWith2FAAsync(enable2Fa: true);
        var loginRequest = new LoginRequest(user.Email, password, TrustedDeviceToken: null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.True(body.GetProperty("requiresTwoFactor").GetBoolean());
        Assert.True(body.TryGetProperty("twoFactorToken", out var twoFactorTokenProp));
        Assert.False(string.IsNullOrWhiteSpace(twoFactorTokenProp.GetString()));

        // No regular tokens issued yet
        Assert.True(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
        Assert.True(string.IsNullOrEmpty(body.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Scenario2_Verify2FA_WithRememberDeviceTrue_IssuesTrustedDeviceToken_AndPersistsInDatabase()
    {
        // Arrange
        var (user, password, secret) = await CreateUserWith2FAAsync(enable2Fa: true);

        // Step 1: Login to get temporary 2FA token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var twoFactorToken = loginBody.GetProperty("twoFactorToken").GetString();

        var validCode = GenerateTotpCode(secret);

        // Act: Verify 2FA with RememberDevice = true
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCode, RememberDevice: true))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorToken);

        var verifyResponse = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.True(verifyBody.TryGetProperty("accessToken", out var accessTokenProp) && !string.IsNullOrWhiteSpace(accessTokenProp.GetString()));
        Assert.True(verifyBody.TryGetProperty("refreshToken", out var refreshTokenProp) && !string.IsNullOrWhiteSpace(refreshTokenProp.GetString()));
        Assert.True(verifyBody.TryGetProperty("trustedDeviceToken", out var trustedTokenProp) && !string.IsNullOrWhiteSpace(trustedTokenProp.GetString()));

        // Assert in real PostgreSQL database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var devicesInDb = await dbContext.TrustedDevices.Where(d => d.UserId == user.Id).ToListAsync();
        Assert.NotEmpty(devicesInDb);
        Assert.True(devicesInDb.First().IsActive);
    }

    [Fact]
    public async Task Scenario3_SecondLogin_WithValidTrustedDeviceToken_BypassesTwoFactor()
    {
        // Arrange
        var (user, password, secret) = await CreateUserWith2FAAsync(enable2Fa: true);

        // Step 1: Login and verify 2FA to acquire trusted device token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var twoFactorToken = loginBody.GetProperty("twoFactorToken").GetString();

        var validCode = GenerateTotpCode(secret);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCode, RememberDevice: true))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorToken);
        var verifyResponse = await _client.SendAsync(request);
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var trustedDeviceToken = verifyBody.GetProperty("trustedDeviceToken").GetString();

        // Act: Second login presenting the trusted device token
        var secondLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            user.Email, password, TrustedDeviceToken: trustedDeviceToken));

        // Assert: 2FA challenge is bypassed, active session is granted
        Assert.Equal(HttpStatusCode.OK, secondLoginResponse.StatusCode);
        var secondLoginBody = await secondLoginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.False(secondLoginBody.GetProperty("requiresTwoFactor").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(secondLoginBody.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(secondLoginBody.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Scenario4_RevokeTrustedDevice_ViaHttpEndpoint_ReturnsNoContent()
    {
        // Arrange: User with trusted device
        var (user, password, secret) = await CreateUserWith2FAAsync(enable2Fa: true);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var twoFactorToken = (await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("twoFactorToken").GetString();

        var validCode = GenerateTotpCode(secret);

        var verifyReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCode, RememberDevice: true))
        };
        verifyReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorToken);
        var verifyRes = await _client.SendAsync(verifyReq);
        var verifyBody = await verifyRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = verifyBody.GetProperty("accessToken").GetString();

        // Retrieve trusted device ID via GET /api/auth/trusted-devices
        var getReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/trusted-devices");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var getRes = await _client.SendAsync(getReq);
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var devices = await getRes.Content.ReadFromJsonAsync<List<TrustedDeviceResponse>>(JsonOptions);
        Assert.NotNull(devices);
        Assert.Single(devices);
        var deviceId = devices[0].Id;

        // Act: DELETE /api/auth/trusted-devices/{deviceId}
        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/auth/trusted-devices/{deviceId}");
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteRes = await _client.SendAsync(deleteReq);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // Verify state in PostgreSQL
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var dbDevice = await context.TrustedDevices.FindAsync(deviceId);
        Assert.NotNull(dbDevice);
        Assert.True(dbDevice.IsRevoked);
        Assert.False(dbDevice.IsActive);
    }

    [Fact]
    public async Task Scenario5_LoginWithRevokedTrustedDeviceToken_DemandsTwoFactorVerificationAgain()
    {
        // Arrange
        var (user, password, secret) = await CreateUserWith2FAAsync(enable2Fa: true);

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var twoFactorToken = (await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("twoFactorToken").GetString();

        var validCode = GenerateTotpCode(secret);

        var verifyReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCode, RememberDevice: true))
        };
        verifyReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorToken);
        var verifyRes = await _client.SendAsync(verifyReq);
        var verifyBody = await verifyRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = verifyBody.GetProperty("accessToken").GetString();
        var trustedDeviceToken = verifyBody.GetProperty("trustedDeviceToken").GetString();

        // Find device id and revoke it
        var getReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/trusted-devices");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var getRes = await _client.SendAsync(getReq);
        var devices = await getRes.Content.ReadFromJsonAsync<List<TrustedDeviceResponse>>(JsonOptions);
        var deviceId = devices![0].Id;

        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/auth/trusted-devices/{deviceId}");
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(deleteReq);

        // Act: Attempt login with the revoked trusted device token
        var postRevocationLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            user.Email, password, TrustedDeviceToken: trustedDeviceToken));

        // Assert: Bypass rejected, 2FA required again
        Assert.Equal(HttpStatusCode.OK, postRevocationLogin.StatusCode);
        var postRevocationBody = await postRevocationLogin.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(postRevocationBody.GetProperty("requiresTwoFactor").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(postRevocationBody.GetProperty("twoFactorToken").GetString()));
    }

    [Fact]
    public async Task Scenario6_Temporary2FAToken_UsedOnBusinessOrProtectedEndpoint_IsRejectedWithUnauthorized()
    {
        // Arrange
        var (user, password, _) = await CreateUserWith2FAAsync(enable2Fa: true);

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var twoFactorToken = (await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("twoFactorToken").GetString();

        // Act: Attempt to access protected endpoint GET /api/auth/me using temporary 2FA token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorToken);
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Scenario7_TrustedDeviceToken_UsedAsBearerAccessToken_IsRejectedWithUnauthorized()
    {
        // Arrange
        var (user, password, secret) = await CreateUserWith2FAAsync(enable2Fa: true);

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var twoFactorToken = (await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("twoFactorToken").GetString();

        var validCode = GenerateTotpCode(secret);

        var verifyReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCode, RememberDevice: true))
        };
        verifyReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorToken);
        var verifyRes = await _client.SendAsync(verifyReq);
        var verifyBody = await verifyRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var trustedDeviceToken = verifyBody.GetProperty("trustedDeviceToken").GetString();

        // Act: Attempt to use trusted device token as Bearer access token on GET /api/auth/me
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trustedDeviceToken);
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Scenario8_UserCannotListOrRevokeDevicesBelongingToAnotherUser()
    {
        // Arrange: Create User A and User B
        var (userA, passwordA, secretA) = await CreateUserWith2FAAsync(enable2Fa: true);
        var (userB, passwordB, secretB) = await CreateUserWith2FAAsync(enable2Fa: true);

        // User A acquires device and token
        var loginA = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(userA.Email, passwordA, null));
        var twoFactorA = (await loginA.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("twoFactorToken").GetString();

        var validCodeA = GenerateTotpCode(secretA);

        var verifyReqA = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCodeA, RememberDevice: true))
        };
        verifyReqA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorA);
        var verifyResA = await _client.SendAsync(verifyReqA);
        var verifyBodyA = await verifyResA.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessA = verifyBodyA.GetProperty("accessToken").GetString();

        // Retrieve User A's device ID
        var getReqA = new HttpRequestMessage(HttpMethod.Get, "/api/auth/trusted-devices");
        getReqA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessA);
        var getResA = await _client.SendAsync(getReqA);
        var devicesA = await getResA.Content.ReadFromJsonAsync<List<TrustedDeviceResponse>>(JsonOptions);
        var deviceIdA = devicesA![0].Id;

        // User B authenticates
        var loginB = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(userB.Email, passwordB, null));
        var twoFactorB = (await loginB.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("twoFactorToken").GetString();

        var validCodeB = GenerateTotpCode(secretB);
        var verifyReqB = new HttpRequestMessage(HttpMethod.Post, "/api/auth/2fa/verify")
        {
            Content = JsonContent.Create(new VerifyTwoFactorAuthRequest(validCodeB, RememberDevice: false))
        };
        verifyReqB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twoFactorB);
        var verifyResB = await _client.SendAsync(verifyReqB);
        var accessB = (await verifyResB.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("accessToken").GetString();

        // Act 1: User B calls GET /api/auth/trusted-devices -> Must NOT see User A's device
        var getReqB = new HttpRequestMessage(HttpMethod.Get, "/api/auth/trusted-devices");
        getReqB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessB);
        var getResB = await _client.SendAsync(getReqB);
        var devicesB = await getResB.Content.ReadFromJsonAsync<List<TrustedDeviceResponse>>(JsonOptions);
        Assert.Empty(devicesB!);

        // Act 2: User B attempts to DELETE User A's device
        var deleteReqB = new HttpRequestMessage(HttpMethod.Delete, $"/api/auth/trusted-devices/{deviceIdA}");
        deleteReqB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessB);
        var deleteResB = await _client.SendAsync(deleteReqB);

        // Assert: 404 NotFound returned without altering User A's device
        Assert.Equal(HttpStatusCode.NotFound, deleteResB.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var deviceStillActive = await context.TrustedDevices.FindAsync(deviceIdA);
        Assert.NotNull(deviceStillActive);
        Assert.True(deviceStillActive.IsActive);
        Assert.False(deviceStillActive.IsRevoked);
    }

    [Fact]
    public async Task Scenario9_LogoutAndRefreshTokenRotation_PreserveSecurityInvariants()
    {
        // Arrange: User with standard login (2FA disabled for this test)
        var (user, password, _) = await CreateUserWith2FAAsync(enable2Fa: false);

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.Email, password, null));
        var loginBody = await loginRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var refreshToken1 = loginBody.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(refreshToken1));

        // Act 1: Rotate refresh token via POST /api/auth/refresh
        var refreshRes = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshToken1));
        Assert.Equal(HttpStatusCode.OK, refreshRes.StatusCode);

        var refreshBody = await refreshRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var refreshToken2 = refreshBody.GetProperty("refreshToken").GetString();
        Assert.NotEqual(refreshToken1, refreshToken2);

        // Invariant: The old refresh token cannot be used again
        var reuseOldTokenRes = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshToken1));
        Assert.Equal(HttpStatusCode.Unauthorized, reuseOldTokenRes.StatusCode);

        // Act 2: Logout using the new active refresh token
        var logoutRes = await _client.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(refreshToken2!));
        Assert.Equal(HttpStatusCode.NoContent, logoutRes.StatusCode);

        // Invariant: Post-logout refresh must fail
        var postLogoutRefreshRes = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshToken2!));
        Assert.Equal(HttpStatusCode.Unauthorized, postLogoutRefreshRes.StatusCode);
    }
}
