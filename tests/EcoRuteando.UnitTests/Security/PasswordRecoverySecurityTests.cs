using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class PasswordRecoverySecurityTests
{
    private readonly Mock<IPasswordRecoveryRepository> _recoveryRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ISessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<ITrustedDeviceService> _trustedDeviceServiceMock = new();
    private readonly Mock<ISecurityUnitOfWork> _uowMock = new();
    private readonly Mock<IAuditLogService> _auditLogMock = new();

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static User CreateTestUser(string email = "test@ecoruteando.com")
    {
        return new User(
            firstName: "Carlos",
            lastName: "Pérez",
            email: email,
            passwordHash: "OldPasswordHash_12345",
            phoneNumber: null);
    }

    private static PasswordRecovery CreateRecovery(User user, string plainToken, int expireMinutes = 15, bool isUsed = false)
    {
        var recovery = new PasswordRecovery(
            userId: user.Id,
            tokenHash: HashToken(plainToken),
            expiresAt: DateTime.UtcNow.AddMinutes(expireMinutes),
            requestIp: IPAddress.Loopback);

        if (isUsed)
        {
            recovery.MarkAsUsed(IPAddress.Loopback);
        }

        typeof(PasswordRecovery).GetProperty("User")?.SetValue(recovery, user);
        return recovery;
    }

    // ── H-41: Vinculación estricta identidad - token ───────────────

    [Fact]
    public async Task ResetPassword_WithMatchingEmail_ShouldSucceedAndChangePassword()
    {
        var user = CreateTestUser("alice@ecoruteando.com");
        var token = "123456";
        var recovery = CreateRecovery(user, token);

        _recoveryRepoMock.Setup(r => r.GetByTokenHashAsync(HashToken(token)))
            .ReturnsAsync(recovery);
        _recoveryRepoMock.Setup(r => r.GetByUserIdAsync(user.Id))
            .ReturnsAsync(new List<PasswordRecovery> { recovery });
        _passwordHasherMock.Setup(h => h.Hash("NewSecurePassword123!"))
            .Returns("Hashed_NewSecurePassword123!");

        var handler = new ResetPasswordCommandHandler(
            _recoveryRepoMock.Object,
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object,
            _auditLogMock.Object);

        var command = new ResetPasswordCommand(
            Token: token,
            NewPassword: "NewSecurePassword123!",
            UsageIp: "127.0.0.1",
            Email: "alice@ecoruteando.com");

        await handler.Handle(command, CancellationToken.None);

        Assert.True(recovery.IsUsed);
        Assert.Equal("Hashed_NewSecurePassword123!", user.PasswordHash);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WithMismatchedEmail_ShouldThrowUnauthorizedExceptionAndNotChangePassword()
    {
        var user = CreateTestUser("alice@ecoruteando.com");
        var token = "123456";
        var recovery = CreateRecovery(user, token);

        _recoveryRepoMock.Setup(r => r.GetByTokenHashAsync(HashToken(token)))
            .ReturnsAsync(recovery);

        var handler = new ResetPasswordCommandHandler(
            _recoveryRepoMock.Object,
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object,
            _auditLogMock.Object);

        var command = new ResetPasswordCommand(
            Token: token,
            NewPassword: "NewSecurePassword123!",
            UsageIp: "127.0.0.1",
            Email: "bob@ecoruteando.com"); // Mismatched email!

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("no corresponde", ex.Message);
        Assert.False(recovery.IsUsed);
        Assert.Equal("OldPasswordHash_12345", user.PasswordHash);
    }

    // ── H-60: Prevención de reutilización y expiración ──────────────

    [Fact]
    public async Task ResetPassword_WhenTokenAlreadyUsed_ShouldThrowUnauthorizedException()
    {
        var user = CreateTestUser("alice@ecoruteando.com");
        var token = "123456";
        var recovery = CreateRecovery(user, token, isUsed: true);

        _recoveryRepoMock.Setup(r => r.GetByTokenHashAsync(HashToken(token)))
            .ReturnsAsync(recovery);

        var handler = new ResetPasswordCommandHandler(
            _recoveryRepoMock.Object,
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object,
            _auditLogMock.Object);

        var command = new ResetPasswordCommand(
            Token: token,
            NewPassword: "NewSecurePassword123!",
            UsageIp: "127.0.0.1");

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("utilizado", ex.Message);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenExpired_ShouldThrowUnauthorizedException()
    {
        var user = CreateTestUser("alice@ecoruteando.com");
        var token = "123456";
        var recovery = CreateRecovery(user, token, expireMinutes: -10); // Expired 10 min ago

        _recoveryRepoMock.Setup(r => r.GetByTokenHashAsync(HashToken(token)))
            .ReturnsAsync(recovery);

        var handler = new ResetPasswordCommandHandler(
            _recoveryRepoMock.Object,
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _sessionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _trustedDeviceServiceMock.Object,
            _uowMock.Object,
            _auditLogMock.Object);

        var command = new ResetPasswordCommand(
            Token: token,
            NewPassword: "NewSecurePassword123!",
            UsageIp: "127.0.0.1");

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("expirado", ex.Message);
    }

    [Fact]
    public async Task ForgotPassword_ShouldInvalidatePreviouslyActiveTokens()
    {
        var user = CreateTestUser("alice@ecoruteando.com");
        var oldToken1 = CreateRecovery(user, "111111", expireMinutes: 10, isUsed: false);
        var oldToken2 = CreateRecovery(user, "222222", expireMinutes: 10, isUsed: false);

        _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _recoveryRepoMock.Setup(r => r.GetByUserIdAsync(user.Id))
            .ReturnsAsync(new List<PasswordRecovery> { oldToken1, oldToken2 });

        var otpMock = new Mock<IOtpProvider>();
        otpMock.Setup(o => o.GenerateCode()).Returns("333333");

        var emailMock = new Mock<IEmailService>();
        var templateMock = new Mock<IEmailTemplateService>();
        templateMock.Setup(t => t.LoadTemplate(It.IsAny<string>()))
            .Returns("Template {{UserName}} {{Code}}");

        var rateLimiterMock = new Mock<IForgotPasswordRateLimiter>();

        var handler = new ForgotPasswordCommandHandler(
            _userRepoMock.Object,
            _recoveryRepoMock.Object,
            _uowMock.Object,
            otpMock.Object,
            emailMock.Object,
            templateMock.Object,
            _auditLogMock.Object,
            rateLimiterMock.Object);

        await handler.Handle(new ForgotPasswordCommand(user.Email, "127.0.0.1"), CancellationToken.None);

        // Previous tokens must now be marked as used!
        Assert.True(oldToken1.IsUsed);
        Assert.True(oldToken2.IsUsed);

        // Verify update was called for the old tokens
        _recoveryRepoMock.Verify(r => r.Update(oldToken1), Times.Once);
        _recoveryRepoMock.Verify(r => r.Update(oldToken2), Times.Once);

        // Verify new token was added
        _recoveryRepoMock.Verify(r => r.AddAsync(It.IsNotNull<PasswordRecovery>()), Times.Once);
    }
}
