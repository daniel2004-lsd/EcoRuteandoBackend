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
using EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
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

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("CurrentPassword123!", user.PasswordHash))
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
}
