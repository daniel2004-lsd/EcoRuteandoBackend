using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed class DisableTwoFactorAuthCommandHandler
    : IRequestHandler<DisableTwoFactorAuthCommand, bool>
{
    private readonly ITotpService _totpService;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public DisableTwoFactorAuthCommandHandler(
        ITotpService totpService,
        ITwoFactorAuthRepository twoFactorAuthRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _totpService = totpService;
        _twoFactorAuthRepository = twoFactorAuthRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<bool> Handle(
        DisableTwoFactorAuthCommand request,
        CancellationToken cancellationToken)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAndMethodAsync(
            request.UserId, TwoFactorMethod.TOTP, cancellationToken);

        if (twoFactorAuth is null || !twoFactorAuth.IsActive)
        {
            throw new NotFoundException("2FA no está activo.");
        }

        if (twoFactorAuth.EncryptedSecret is not null)
        {
            var secret = Encoding.UTF8.GetString(twoFactorAuth.EncryptedSecret);

            if (!_totpService.ValidateCode(secret, request.Code))
            {
                throw new UnauthorizedException("Código 2FA inválido.");
            }
        }

        twoFactorAuth.Disable();
        _twoFactorAuthRepository.Update(twoFactorAuth);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            request.UserId,
            "2fa.disabled",
            entityName: "two_factor_auth",
            entityId: request.UserId.ToString(),
            cancellationToken: cancellationToken);

        return true;
    }
}
