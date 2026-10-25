using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EcoRuteando.IntegrationTests.Infrastructure;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyProfile;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoRuteando.IntegrationTests.Security;

public sealed class ProfileEmailChangeE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProfileEmailChangeE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(User User, string RawPassword, string AccessToken)> CreateActiveUserWithTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        var rawPassword = "ProfilePass123!";
        var email = $"profile_{Guid.NewGuid():N}@test.ecoruteando.com";

        var user = new User(
            firstName: "Profile",
            lastName: "Tester",
            email: email,
            passwordHash: passwordHasher.Hash(rawPassword),
            phoneNumber: "+573001112233");

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

        var session = new Session(
            user.Id,
            refreshTokenHash: "dummy_profile_refresh_hash_" + Guid.NewGuid().ToString("N"),
            sourceIp: null,
            userAgent: "TestAgent",
            device: null,
            expiresAt: DateTime.UtcNow.AddDays(7));

        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var accessToken = jwtProvider.GenerateToken(user.Id, user.Email, regularRole.Name, session.Id);

        return (user, rawPassword, accessToken);
    }

    [Fact]
    public async Task UpdateProfile_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var request = new UpdateMyProfileRequest(
            FirstName: "Hack",
            LastName: "Attempt",
            Email: "hacked@ecoruteando.com",
            PhoneNumber: null,
            CurrentPassword: "AnyPassword123!");

        // Act
        var response = await _client.PutAsJsonAsync("/api/users/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WhenPasswordIsIncorrect_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, token) = await CreateActiveUserWithTokenAsync();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, "/api/users/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        requestMessage.Content = JsonContent.Create(new UpdateMyProfileRequest(
            FirstName: "Profile",
            LastName: "Tester",
            Email: "new_email@ecoruteando.com",
            PhoneNumber: null,
            CurrentPassword: "WrongPassword123!"));

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("La contraseña actual es incorrecta.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateProfile_WhenEmailAlreadyInUse_ReturnsConflict()
    {
        // Arrange
        var (user1, pass1, token1) = await CreateActiveUserWithTokenAsync();
        var (user2, _, _) = await CreateActiveUserWithTokenAsync();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, "/api/users/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        requestMessage.Content = JsonContent.Create(new UpdateMyProfileRequest(
            FirstName: "Profile",
            LastName: "Tester",
            Email: user2.Email, // Email taken by user2
            PhoneNumber: null,
            CurrentPassword: pass1));

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var conflictProblem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("El correo no está disponible.", conflictProblem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateProfile_WhenEmailNotChanged_PreservesVerifiedStatus()
    {
        // Arrange
        var (user, password, token) = await CreateActiveUserWithTokenAsync();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, "/api/users/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        requestMessage.Content = JsonContent.Create(new UpdateMyProfileRequest(
            FirstName: "UpdatedFirstName",
            LastName: "UpdatedLastName",
            Email: user.Email, // Same email
            PhoneNumber: "+573009999999",
            CurrentPassword: password));

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify in DB that EmailVerified is still true
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var userInDb = await context.Users.FirstAsync(u => u.Id == user.Id);
        Assert.True(userInDb.EmailVerified, "EmailVerified must remain true when email has not changed");
        Assert.Equal("UpdatedFirstName", userInDb.FirstName);
    }

    [Fact]
    public async Task UpdateProfile_WhenEmailChanges_ResetsVerifiedAndRequiresVerificationOnLogin()
    {
        // Arrange
        var (user, password, token) = await CreateActiveUserWithTokenAsync();
        var newEmail = $"new_{Guid.NewGuid():N}@test.ecoruteando.com";

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/users/me");
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        updateRequest.Content = JsonContent.Create(new UpdateMyProfileRequest(
            FirstName: "NewName",
            LastName: "NewLast",
            Email: newEmail,
            PhoneNumber: "+573008887766",
            CurrentPassword: password));

        // Act 1: Update email
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Assert 1: EmailVerified in DB is now false
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
            var updatedUser = await context.Users.FirstAsync(u => u.Id == user.Id);
            Assert.Equal(newEmail, updatedUser.Email);
            Assert.False(updatedUser.EmailVerified, "EmailVerified must be false after email change to prevent account takeover");
        }

        // Act 2: Attempting to log in with new email before verification
        var loginAttemptResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            Email: newEmail,
            Password: password));

        Assert.Equal(HttpStatusCode.OK, loginAttemptResponse.StatusCode);
        var loginAttemptBody = await loginAttemptResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(loginAttemptBody);
        Assert.True(loginAttemptBody.RequiresEmailVerification, "Login must require email verification");
        Assert.True(string.IsNullOrEmpty(loginAttemptBody.AccessToken), "Access token must be empty when unverified");

        // Act 3: Complete verification with the OTP code generated for this user
        // We find the code that was created during UpdateProfile
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
            var activeVerification = await context.EmailVerifications
                .Where(v => v.UserId == user.Id && !v.IsVerified)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            Assert.NotNull(activeVerification);

            // Simulate verifying the email via VerifyEmail method
            var userInDb = await context.Users.FirstAsync(u => u.Id == user.Id);
            userInDb.VerifyEmail();
            activeVerification.MarkAsVerified();
            await context.SaveChangesAsync();
        }

        // Act 4: Login again now that email is verified
        var verifiedLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            Email: newEmail,
            Password: password));

        Assert.Equal(HttpStatusCode.OK, verifiedLoginResponse.StatusCode);
        var verifiedLoginBody = await verifiedLoginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(verifiedLoginBody);
        Assert.False(verifiedLoginBody.RequiresEmailVerification);
        Assert.False(string.IsNullOrWhiteSpace(verifiedLoginBody.AccessToken));

        // Act 5: Call GET /api/auth/me with the new JWT
        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", verifiedLoginBody.AccessToken);

        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meBody = await meResponse.Content.ReadFromJsonAsync<CurrentUserResponse>(JsonOptions);
        Assert.NotNull(meBody);
        Assert.Equal(newEmail, meBody.Email);
    }
}
