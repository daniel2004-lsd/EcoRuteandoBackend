using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand>
{
    private readonly IPasswordRecoveryRepository _passwordRecoveryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public ResetPasswordCommandHandler(
        IPasswordRecoveryRepository passwordRecoveryRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITrustedDeviceService trustedDeviceService,
        ISecurityUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _passwordRecoveryRepository = passwordRecoveryRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _trustedDeviceService = trustedDeviceService;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256(request.Token);
        var recovery = await _passwordRecoveryRepository.GetByTokenHashAsync(tokenHash);

        if (recovery is null)
        {
            throw new UnauthorizedException("Token inválido.");
        }

        if (recovery.IsUsed)
        {
            throw new UnauthorizedException("El token ya fue utilizado.");
        }

        if (recovery.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("El token ha expirado.");
        }

        var user = recovery.User;

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        // Regla de vinculación estricta de identidad token-usuario (H-41)
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("El código no corresponde al correo indicado.");
        }

        IPAddress? usageIp = null;

        if (!string.IsNullOrWhiteSpace(request.UsageIp))
        {
            IPAddress.TryParse(request.UsageIp, out usageIp);
        }

        var passwordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(passwordHash);

        // Invalidar inmediatamente el token consumido (H-60)
        recovery.MarkAsUsed(usageIp);
        _passwordRecoveryRepository.Update(recovery);

        // Invalidar de forma atómica cualquier otro token de recuperación pendiente para este usuario
        var activeTokens = await _passwordRecoveryRepository.GetByUserIdAsync(user.Id);
        foreach (var extra in activeTokens.Where(t => !t.IsUsed && t.Id != recovery.Id))
        {
            extra.MarkAsUsed(usageIp);
            _passwordRecoveryRepository.Update(extra);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidar sesiones activas del usuario (H-23 / H-24)
        var sessions = await _sessionRepository.GetActiveByUserIdAsync(
            user.Id, cancellationToken);
        if (sessions is not null)
        {
            foreach (var session in sessions)
            {
                session.Revoke();
                _sessionRepository.Update(session);
            }
        }

        // Invalidar refresh tokens activos del usuario (H-23 / H-24)
        var refreshTokens = await _refreshTokenRepository.GetByUserIdAsync(
            user.Id, cancellationToken);
        if (refreshTokens is not null)
        {
            foreach (var rt in refreshTokens.Where(t => t.IsActive))
            {
                rt.Revoke(revokedByIp: request.UsageIp, replacedByRefreshTokenHash: null);
                await _refreshTokenRepository.UpdateAsync(rt, cancellationToken);
            }
        }

        // Invalidar dispositivos confiables del usuario (H-23 / H-24)
        if (_trustedDeviceService is not null)
        {
            await _trustedDeviceService.RevokeAllForUserAsync(user.Id, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "user.password_reset",
            entityName: "users",
            entityId: user.Id.ToString(),
            sourceIp: request.UsageIp,
            cancellationToken: cancellationToken);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}
