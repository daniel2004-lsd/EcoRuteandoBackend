using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ISecurityPolicyRepository securityPolicyRepository,
        ISessionRepository sessionRepository,
        ITwoFactorAuthRepository twoFactorAuthRepository,
        ITotpService totpService,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _securityPolicyRepository = securityPolicyRepository;
        _sessionRepository = sessionRepository;
        _twoFactorAuthRepository = twoFactorAuthRepository;
        _totpService = totpService;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<LoginResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Correo o contraseña incorrectos.");
        }

        var policy = await _securityPolicyRepository.GetAsync(cancellationToken);

        if (user.IsLocked)
        {
            throw new UnauthorizedException(
                "La cuenta está bloqueada temporalmente. Intente de nuevo más tarde.");
        }

        var isPasswordValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            var maxAttempts = policy?.MaxFailedAttempts ?? 5;
            var lockoutMinutes = policy?.LockoutTimeMinutes ?? 30;

            user.IncrementFailedAttempts(maxAttempts, lockoutMinutes);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                user.Id,
                "user.login_failed",
                entityName: "users",
                entityId: user.Id.ToString(),
                cancellationToken: cancellationToken);

            throw new UnauthorizedException("Correo o contraseña incorrectos.");
        }

        if (!user.EmailVerified)
        {
            throw new ForbiddenException(
                "Debes verificar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada.");
        }

        if (user.PrimaryRole is null)
        {
            throw new DomainException("El usuario no tiene un rol asignado.");
        }

        user.ResetFailedAttempts();
        user.RecordLogin();

        // Check if 2FA is enabled
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAndMethodAsync(
            user.Id, TwoFactorMethod.TOTP, cancellationToken);

        if (twoFactorAuth is { IsActive: true })
        {
            // Generate a temporary token for 2FA verification
            var twoFactorToken = _jwtProvider.GenerateToken(
                user.Id,
                user.Email,
                "two_factor_pending");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                user.Id,
                "user.login_2fa_required",
                entityName: "users",
                entityId: user.Id.ToString(),
                cancellationToken: cancellationToken);

            return new LoginResponse(
                AccessToken: string.Empty,
                RefreshToken: string.Empty,
                RequiresTwoFactor: true,
                TwoFactorToken: twoFactorToken);
        }

        // No 2FA — proceed with normal login
        var accessToken = _jwtProvider.GenerateToken(
            user.Id,
            user.Email,
            user.PrimaryRole.Name);

        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenService.ComputeHash(refreshToken);

        var refreshTokenEntity = new EcoRuteando.Modules.Security.Domain.Entities.RefreshToken(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(7),
            null);

        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity,
            cancellationToken);

        // Enforce max active sessions policy (revokes oldest sessions)
        await EnforceMaxActiveSessionsAsync(user.Id, policy, cancellationToken);

        // Create session
        var session = new Session(
            user.Id,
            refreshTokenHash,
            null,
            null,
            null,
            DateTime.UtcNow.AddDays(7));

        await _sessionRepository.AddAsync(session, cancellationToken);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "user.login_success",
            entityName: "users",
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshToken);
    }

    private async Task EnforceMaxActiveSessionsAsync(
        Guid userId,
        SecurityPolicy? policy,
        CancellationToken cancellationToken)
    {
        var maxActiveSessions = policy?.MaxActiveSessions ?? 0;

        if (maxActiveSessions <= 0)
        {
            return;
        }

        var activeSessions = await _sessionRepository.GetActiveByUserIdAsync(
            userId,
            cancellationToken);

        var excessCount = activeSessions.Count - maxActiveSessions + 1;

        if (excessCount <= 0)
        {
            return;
        }

        foreach (var session in activeSessions
            .OrderBy(s => s.CreatedAt)
            .Take(excessCount))
        {
            session.Revoke();
            _sessionRepository.Update(session);
        }
    }
}
