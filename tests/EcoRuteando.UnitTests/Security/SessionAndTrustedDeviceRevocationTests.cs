using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.ChangeMyPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;
using EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;
using EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;
using EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Infrastructure;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class SessionAndTrustedDeviceRevocationTests
{
    private readonly Mock<ISessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<ITrustedDeviceRepository> _trustedDeviceRepoMock = new();
    private readonly Mock<ITrustedDeviceService> _trustedDeviceServiceMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IPasswordRecoveryRepository> _recoveryRepoMock = new();
    private readonly Mock<ITwoFactorAuthRepository> _twoFactorAuthRepoMock = new();
    private readonly Mock<ITotpService> _totpServiceMock = new();
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<ISecurityUnitOfWork> _uowMock = new();
    private readonly Mock<IAuditLogService> _auditLogMock = new();

    private static User CreateTestUser(Guid? id = null)
    {
        var user = new User(
            firstName: "Carlos",
            lastName: "Pérez",
            email: "carlos@ecoruteando.com",
            passwordHash: "CurrentHash123",
            phoneNumber: null);

        if (id.HasValue)
        {
            typeof(User).GetProperty("Id")?.SetValue(user, id.Value);
        }

        var role = new Role("user", "Regular user");
        typeof(User).GetProperty("PrimaryRole")?.SetValue(user, role);

        return user;
    }

    [Fact]
    public async Task RevokeSession_ShouldRevokeAssociatedRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        const string tokenHash = "hash_token_abc";

        var session = new Session(userId, tokenHash, IPAddress.Loopback, "Mozilla", "Chrome", DateTime.UtcNow.AddDays(7));
        typeof(Session).GetProperty("Id")?.SetValue(session, sessionId);

        var refreshToken = new RefreshToken(userId, tokenHash, DateTime.UtcNow.AddDays(7), "127.0.0.1");

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _refreshTokenRepoMock.Setup(r => r.GetByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var handler = new RevokeSessionCommandHandler(
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _uowMock.Object);

        // Act
        await handler.Handle(new RevokeSessionCommand(userId, sessionId), CancellationToken.None);

        // Assert
        Assert.False(session.IsActive, "Session must be marked inactive");
        Assert.NotNull(session.RevokedAt);

        Assert.False(refreshToken.IsActive, "Refresh token must be revoked");
        Assert.True(refreshToken.IsRevoked);

        _sessionRepoMock.Verify(r => r.Update(session), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAllSessions_ShouldRevokeAllRefreshTokensAndTrustedDevices()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var session1 = new Session(userId, "hash1", IPAddress.Loopback, "Agent", "PC", DateTime.UtcNow.AddDays(7));
        var session2 = new Session(userId, "hash2", IPAddress.Loopback, "Agent", "Phone", DateTime.UtcNow.AddDays(7));

        var rt1 = new RefreshToken(userId, "hash1", DateTime.UtcNow.AddDays(7), "127.0.0.1");
        var rt2 = new RefreshToken(userId, "hash2", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        _sessionRepoMock.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { session1, session2 });

        _refreshTokenRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { rt1, rt2 });

        var handler = new RevokeAllSessionsCommandHandler(
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object);

        // Act
        var revokedCount = await handler.Handle(new RevokeAllSessionsCommand(userId), CancellationToken.None);

        // Assert
        Assert.Equal(2, revokedCount);
        Assert.False(session1.IsActive);
        Assert.False(session2.IsActive);
        Assert.False(rt1.IsActive);
        Assert.False(rt2.IsActive);

        _trustedDeviceServiceMock.Verify(t => t.RevokeAllForUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WhenAssociatedSessionIsRevoked_ShouldThrowUnauthorizedExceptionAndRevokeToken()
    {
        // Arrange
        var user = CreateTestUser();
        const string rawToken = "raw_refresh_token_123";
        const string tokenHash = "computed_hash_123";

        var refreshToken = new RefreshToken(user.Id, tokenHash, DateTime.UtcNow.AddDays(7), "127.0.0.1");
        typeof(RefreshToken).GetProperty("User")?.SetValue(refreshToken, user);

        var session = new Session(user.Id, tokenHash, IPAddress.Loopback, "Browser", "PC", DateTime.UtcNow.AddDays(7));
        session.Revoke(); // Revoked session!

        _refreshTokenServiceMock.Setup(s => s.ComputeHash(rawToken)).Returns(tokenHash);
        _refreshTokenRepoMock.Setup(r => r.GetByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _sessionRepoMock.Setup(r => r.GetByRefreshTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new RefreshTokenCommandHandler(
            _refreshTokenRepoMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtProviderMock.Object,
            _sessionRepoMock.Object,
            _uowMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None));

        Assert.Contains("revocada", ex.Message);
        Assert.False(refreshToken.IsActive, "Refresh token must be revoked when session was already revoked");
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeMyPassword_ShouldRevokeAllSessionsRefreshTokensAndTrustedDevices()
    {
        // Arrange
        var user = CreateTestUser();
        var currentHash = user.PasswordHash;
        Assert.NotNull(currentHash);

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("CurrentPassword123!", currentHash))
            .Returns(true);
        _passwordHasherMock.Setup(h => h.Hash("NewPassword456!"))
            .Returns("NewHashedPassword456");

        var session = new Session(user.Id, "hash_s", IPAddress.Loopback, "Agent", "PC", DateTime.UtcNow.AddDays(7));
        var rt = new RefreshToken(user.Id, "hash_s", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        _sessionRepoMock.Setup(r => r.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { session });
        _refreshTokenRepoMock.Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { rt });

        var handler = new ChangeMyPasswordCommandHandler(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object);

        // Act
        await handler.Handle(
            new ChangeMyPasswordCommand(user.Id, "CurrentPassword123!", "NewPassword456!"),
            CancellationToken.None);

        // Assert
        Assert.False(session.IsActive);
        Assert.False(rt.IsActive);
        _trustedDeviceServiceMock.Verify(t => t.RevokeAllForUserAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisableTwoFactorAuth_ShouldRevokeAllTrustedDevices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var twoFactor = new TwoFactorAuth(userId, TwoFactorMethod.TOTP, encryptedSecret: null);
        twoFactor.Enable();

        _twoFactorAuthRepoMock.Setup(r => r.GetByUserIdAndMethodAsync(userId, TwoFactorMethod.TOTP, It.IsAny<CancellationToken>()))
            .ReturnsAsync(twoFactor);

        var handler = new DisableTwoFactorAuthCommandHandler(
            _totpServiceMock.Object,
            _encryptionServiceMock.Object,
            _twoFactorAuthRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object,
            _auditLogMock.Object);

        // Act
        var result = await handler.Handle(new DisableTwoFactorAuthCommand(userId, "123456"), CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.False(twoFactor.IsActive);
        _trustedDeviceServiceMock.Verify(t => t.RevokeAllForUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrustedDeviceService_WhenDeviceIsActive_ShouldValidateSuccessfully()
    {
        // Arrange
        const string secretKey = "a_very_secret_signing_key_for_testing_purposes_only_12345";
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = secretKey,
            Issuer = "EcoRuteando",
            Audience = "EcoRuteandoUsers"
        });

        var userId = Guid.NewGuid();
        TrustedDevice? savedDevice = null;

        _trustedDeviceRepoMock.Setup(r => r.AddAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TrustedDevice, CancellationToken>((d, _) => savedDevice = d)
            .Returns(Task.CompletedTask);

        _trustedDeviceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => savedDevice);

        var service = new TrustedDeviceService(
            jwtOptions,
            _trustedDeviceRepoMock.Object,
            _uowMock.Object);

        // Act - Generate token
        var token = await service.GenerateTrustedDeviceTokenAsync(userId, 30);

        // Assert - Token generation
        Assert.NotNull(token);
        Assert.NotNull(savedDevice);
        Assert.Equal(userId, savedDevice!.UserId);
        Assert.True(savedDevice.IsActive);

        // Act - Validate token
        var isValid = await service.ValidateTrustedDeviceTokenAsync(token, userId);
        Assert.True(isValid, "Active trusted device token must validate successfully");

        // Act - Revoke device in database
        savedDevice.Revoke();

        // Assert - Validation after revocation
        var isValidAfterRevocation = await service.ValidateTrustedDeviceTokenAsync(token, userId);
        Assert.False(isValidAfterRevocation, "Revoked device must be rejected by ValidateTrustedDeviceTokenAsync");
    }

    [Fact]
    public async Task TrustedDeviceService_WhenDeviceNotFoundInDb_ShouldRejectValidation()
    {
        // Arrange
        const string secretKey = "a_very_secret_signing_key_for_testing_purposes_only_12345";
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = secretKey,
            Issuer = "EcoRuteando",
            Audience = "EcoRuteandoUsers"
        });

        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();

        // Manually sign a token that has NO record in DB
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("token_type", "trusted_device"),
            new Claim(JwtRegisteredClaimNames.Jti, deviceId.ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer: "EcoRuteando",
            audience: "EcoRuteandoUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: signingCredentials);

        var tokenWithoutDbRecord = new JwtSecurityTokenHandler().WriteToken(jwt);

        _trustedDeviceRepoMock.Setup(r => r.GetByIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrustedDevice?)null);

        var service = new TrustedDeviceService(
            jwtOptions,
            _trustedDeviceRepoMock.Object,
            _uowMock.Object);

        // Act
        var isValid = await service.ValidateTrustedDeviceTokenAsync(tokenWithoutDbRecord, userId);

        // Assert
        Assert.False(isValid, "Token with no database record must be rejected");
    }

    [Fact]
    public async Task GetTrustedDevicesQueryHandler_ShouldReturnActiveDevicesForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var device1 = new TrustedDevice(Guid.NewGuid(), userId, DateTime.UtcNow.AddDays(30), "Chrome on macOS");
        var device2 = new TrustedDevice(Guid.NewGuid(), userId, DateTime.UtcNow.AddDays(15), "EcoRuteando Android");

        _trustedDeviceRepoMock.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TrustedDevice> { device1, device2 });

        var handler = new GetTrustedDevicesQueryHandler(_trustedDeviceRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetTrustedDevicesQuery(userId), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.DeviceName == "Chrome on macOS");
        Assert.Contains(result, d => d.DeviceName == "EcoRuteando Android");
        Assert.All(result, d => Assert.True(d.IsActive));
        Assert.All(result, d => Assert.False(d.IsRevoked));
    }

    [Fact]
    public async Task RevokeTrustedDeviceCommandHandler_WhenDeviceBelongsToUser_ShouldRevokeAndSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var device = new TrustedDevice(Guid.NewGuid(), userId, DateTime.UtcNow.AddDays(30), "Firefox on Linux");
        var deviceId = device.Id;

        _trustedDeviceRepoMock.Setup(r => r.GetByIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var handler = new RevokeTrustedDeviceCommandHandler(
            _trustedDeviceRepoMock.Object,
            _uowMock.Object);

        // Act
        await handler.Handle(new RevokeTrustedDeviceCommand(userId, deviceId), CancellationToken.None);

        // Assert
        Assert.True(device.IsRevoked);
        Assert.False(device.IsActive);
        Assert.NotNull(device.RevokedAt);
        _trustedDeviceRepoMock.Verify(r => r.Update(device), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeTrustedDeviceCommandHandler_WhenDeviceBelongsToAnotherUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var deviceOfUserB = new TrustedDevice(Guid.NewGuid(), userB, DateTime.UtcNow.AddDays(30), "Safari on iPhone");

        _trustedDeviceRepoMock.Setup(r => r.GetByIdAsync(deviceOfUserB.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deviceOfUserB);

        var handler = new RevokeTrustedDeviceCommandHandler(
            _trustedDeviceRepoMock.Object,
            _uowMock.Object);

        // Act & Assert (User A attempts to revoke User B's device -> must return NotFound, not revealing existence)
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RevokeTrustedDeviceCommand(userA, deviceOfUserB.Id), CancellationToken.None));

        Assert.Contains("no encontrado", ex.Message);
        Assert.False(deviceOfUserB.IsRevoked, "Foreign device must not be revoked");
        _trustedDeviceRepoMock.Verify(r => r.Update(It.IsAny<TrustedDevice>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTrustedDeviceCommandHandler_WhenDeviceNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentDeviceId = Guid.NewGuid();

        _trustedDeviceRepoMock.Setup(r => r.GetByIdAsync(nonExistentDeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrustedDevice?)null);

        var handler = new RevokeTrustedDeviceCommandHandler(
            _trustedDeviceRepoMock.Object,
            _uowMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RevokeTrustedDeviceCommand(userId, nonExistentDeviceId), CancellationToken.None));

        Assert.Contains("no encontrado", ex.Message);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTrustedDeviceCommandHandler_WhenAlreadyRevoked_ShouldBeIdempotent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var device = new TrustedDevice(Guid.NewGuid(), userId, DateTime.UtcNow.AddDays(30), "Chrome on Windows");
        device.Revoke(); // Already revoked
        var firstRevokedAt = device.RevokedAt;

        _trustedDeviceRepoMock.Setup(r => r.GetByIdAsync(device.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var handler = new RevokeTrustedDeviceCommandHandler(
            _trustedDeviceRepoMock.Object,
            _uowMock.Object);

        // Act
        await handler.Handle(new RevokeTrustedDeviceCommand(userId, device.Id), CancellationToken.None);

        // Assert
        Assert.True(device.IsRevoked);
        Assert.Equal(firstRevokedAt, device.RevokedAt); // Timestamp preserved
        _trustedDeviceRepoMock.Verify(r => r.Update(It.IsAny<TrustedDevice>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void JwtProvider_WhenSessionIdProvided_ShouldEmbedSessionIdClaim()
    {
        // Arrange
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "a_very_secret_signing_key_for_testing_purposes_only_12345",
            Issuer = "EcoRuteando",
            Audience = "EcoRuteandoUsers",
            ExpirationMinutes = 15
        });

        var provider = new JwtProvider(jwtOptions);
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        var tokenString = provider.GenerateToken(userId, "test@ecoruteando.com", "user", sessionId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        var sessionClaim = jwt.Claims.FirstOrDefault(c => c.Type == "session_id");
        Assert.NotNull(sessionClaim);
        Assert.Equal(sessionId.ToString(), sessionClaim.Value);
    }

    [Fact]
    public void JwtProvider_WhenSessionIdNotProvided_ShouldNotEmbedSessionIdClaim()
    {
        // Arrange
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "a_very_secret_signing_key_for_testing_purposes_only_12345",
            Issuer = "EcoRuteando",
            Audience = "EcoRuteandoUsers",
            ExpirationMinutes = 15
        });

        var provider = new JwtProvider(jwtOptions);
        var userId = Guid.NewGuid();

        // Act
        var tokenString = provider.GenerateToken(userId, "test@ecoruteando.com", "user");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        var sessionClaim = jwt.Claims.FirstOrDefault(c => c.Type == "session_id");
        Assert.Null(sessionClaim);
    }

    private static IServiceProvider CreateServiceProviderWithSessionRepo(ISessionRepository sessionRepo)
    {
        var services = new ServiceCollection();
        services.AddSingleton(sessionRepo);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "a_very_secret_signing_key_for_testing_purposes_only_12345",
                ["Jwt:Issuer"] = "EcoRuteando",
                ["Jwt:Audience"] = "EcoRuteandoUsers",
                ["Jwt:ExpirationMinutes"] = "15"
            })
            .Build();

        services.AddJwtAuthentication(config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task OnTokenValidated_WhenAccessTokenHasActiveSession_ShouldSucceed()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new Session(userId, "hash", IPAddress.Loopback, "Agent", "PC", DateTime.UtcNow.AddDays(7));
        typeof(Session).GetProperty("Id")?.SetValue(session, sessionId);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sp = CreateServiceProviderWithSessionRepo(_sessionRepoMock.Object);
        var jwtOptions = sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "access"),
            new Claim("session_id", sessionId.ToString())
        };
        var context = new TokenValidatedContext(
            httpContext,
            new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
        };

        // Act
        await jwtOptions.Events.OnTokenValidated(context);

        // Assert
        Assert.Null(context.Result?.Failure);
    }

    [Fact]
    public async Task OnTokenValidated_WhenAccessTokenHasRevokedSession_ShouldFail()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new Session(userId, "hash", IPAddress.Loopback, "Agent", "PC", DateTime.UtcNow.AddDays(7));
        typeof(Session).GetProperty("Id")?.SetValue(session, sessionId);
        session.Revoke();

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sp = CreateServiceProviderWithSessionRepo(_sessionRepoMock.Object);
        var jwtOptions = sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "access"),
            new Claim("session_id", sessionId.ToString())
        };
        var context = new TokenValidatedContext(
            httpContext,
            new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
        };

        // Act
        await jwtOptions.Events.OnTokenValidated(context);

        // Assert
        Assert.NotNull(context.Result?.Failure);
        Assert.Contains("revocada", context.Result!.Failure!.Message);
    }

    [Fact]
    public async Task OnTokenValidated_WhenAccessTokenHasExpiredSession_ShouldFail()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        // Session expired in the past
        var session = new Session(userId, "hash", IPAddress.Loopback, "Agent", "PC", DateTime.UtcNow.AddMinutes(-5));
        typeof(Session).GetProperty("Id")?.SetValue(session, sessionId);

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sp = CreateServiceProviderWithSessionRepo(_sessionRepoMock.Object);
        var jwtOptions = sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "access"),
            new Claim("session_id", sessionId.ToString())
        };
        var context = new TokenValidatedContext(
            httpContext,
            new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
        };

        // Act
        await jwtOptions.Events.OnTokenValidated(context);

        // Assert
        Assert.NotNull(context.Result?.Failure);
        Assert.Contains("expirado", context.Result!.Failure!.Message);
    }

    [Fact]
    public async Task OnTokenValidated_WhenAccessTokenSessionNotFoundInDb_ShouldFail()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        var sp = CreateServiceProviderWithSessionRepo(_sessionRepoMock.Object);
        var jwtOptions = sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "access"),
            new Claim("session_id", sessionId.ToString())
        };
        var context = new TokenValidatedContext(
            httpContext,
            new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
        };

        // Act
        await jwtOptions.Events.OnTokenValidated(context);

        // Assert
        Assert.NotNull(context.Result?.Failure);
        Assert.Contains("revocada", context.Result!.Failure!.Message);
    }

    [Fact]
    public async Task OnTokenValidated_WhenAccessTokenMissingSessionIdClaim_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sp = CreateServiceProviderWithSessionRepo(_sessionRepoMock.Object);
        var jwtOptions = sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "access")
            // No session_id claim
        };
        var context = new TokenValidatedContext(
            httpContext,
            new AuthenticationScheme("Bearer", "Bearer", typeof(JwtBearerHandler)),
            jwtOptions)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
        };

        // Act
        await jwtOptions.Events.OnTokenValidated(context);

        // Assert
        Assert.NotNull(context.Result?.Failure);
        Assert.Contains("sin identificador de sesión", context.Result!.Failure!.Message);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_PassesNewSessionIdToJwtProvider()
    {
        // Arrange
        var user = CreateTestUser();
        const string rawToken = "raw_refresh_token_active";
        const string tokenHash = "computed_hash_active";

        var refreshToken = new RefreshToken(user.Id, tokenHash, DateTime.UtcNow.AddDays(7), "127.0.0.1");
        typeof(RefreshToken).GetProperty("User")?.SetValue(refreshToken, user);

        var session = new Session(user.Id, tokenHash, IPAddress.Loopback, "Browser", "PC", DateTime.UtcNow.AddDays(7));

        _refreshTokenServiceMock.Setup(s => s.ComputeHash(rawToken)).Returns(tokenHash);
        _refreshTokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("new_raw_refresh_token");
        _refreshTokenServiceMock.Setup(s => s.ComputeHash("new_raw_refresh_token")).Returns("new_computed_hash");

        _refreshTokenRepoMock.Setup(r => r.GetByHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _sessionRepoMock.Setup(r => r.GetByRefreshTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _jwtProviderMock.Setup(j => j.GenerateToken(user.Id, user.Email, user.PrimaryRole!.Name, It.IsNotNull<Guid?>()))
            .Returns("new_valid_access_token");

        var handler = new RefreshTokenCommandHandler(
            _refreshTokenRepoMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtProviderMock.Object,
            _sessionRepoMock.Object,
            _uowMock.Object);

        // Act
        var response = await handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        // Assert
        Assert.Equal("new_valid_access_token", response.AccessToken);
        _jwtProviderMock.Verify(j => j.GenerateToken(
            user.Id,
            user.Email,
            user.PrimaryRole!.Name,
            It.Is<Guid?>(id => id.HasValue && id.Value != Guid.Empty)), Times.Once);
        _sessionRepoMock.Verify(r => r.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
