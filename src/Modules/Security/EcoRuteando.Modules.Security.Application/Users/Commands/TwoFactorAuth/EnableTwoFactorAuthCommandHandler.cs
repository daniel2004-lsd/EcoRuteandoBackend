using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed class EnableTwoFactorAuthCommandHandler
    : IRequestHandler<EnableTwoFactorAuthCommand, TwoFactorSetupResponse>
{
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public EnableTwoFactorAuthCommandHandler(
        ITotpService totpService,
        IEncryptionService encryptionService,
        ITwoFactorAuthRepository twoFactorAuthRepository,
        IUserRepository userRepository,
        ISecurityUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _totpService = totpService;
        _encryptionService = encryptionService;
        _twoFactorAuthRepository = twoFactorAuthRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<TwoFactorSetupResponse> Handle(
        EnableTwoFactorAuthCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _twoFactorAuthRepository.GetByUserIdAndMethodAsync(
            request.UserId, TwoFactorMethod.TOTP, cancellationToken);

        if (existing is { IsActive: true })
        {
            throw new ConflictException("2FA ya está activo para esta cuenta.");
        }

        var user = await _userRepository.GetByIdAsync(
            request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        var secret = _totpService.GenerateSecret();
        var qrCodeUri = _totpService.GenerateQrCodeUri(
            secret, user.Email, "EcoRuteando");
        var recoveryCodes = _totpService.GenerateRecoveryCodes();

        var secretBytes = _encryptionService.Encrypt(System.Text.Encoding.UTF8.GetBytes(secret));

        if (existing is not null)
        {
            existing.UpdateSecret(secretBytes);
            _twoFactorAuthRepository.Update(existing);
        }
        else
        {
            var twoFactorAuth = new EcoRuteando.Modules.Security.Domain.Entities.TwoFactorAuth(
                request.UserId,
                TwoFactorMethod.TOTP,
                secretBytes);

            await _twoFactorAuthRepository.AddAsync(
                twoFactorAuth, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            request.UserId,
            "2fa.setup_initiated",
            entityName: "two_factor_auth",
            entityId: request.UserId.ToString(),
            cancellationToken: cancellationToken);

        return new TwoFactorSetupResponse(secret, qrCodeUri, recoveryCodes);
    }
}
