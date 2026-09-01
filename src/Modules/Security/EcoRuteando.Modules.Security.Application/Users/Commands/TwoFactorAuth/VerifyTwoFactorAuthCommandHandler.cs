using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed class VerifyTwoFactorAuthCommandHandler
    : IRequestHandler<VerifyTwoFactorAuthCommand, bool>
{
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public VerifyTwoFactorAuthCommandHandler(
        ITotpService totpService,
        IEncryptionService encryptionService,
        ITwoFactorAuthRepository twoFactorAuthRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _totpService = totpService;
        _encryptionService = encryptionService;
        _twoFactorAuthRepository = twoFactorAuthRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        VerifyTwoFactorAuthCommand request,
        CancellationToken cancellationToken)
    {
        var twoFactorAuth = await _twoFactorAuthRepository.GetByUserIdAndMethodAsync(
            request.UserId, TwoFactorMethod.TOTP, cancellationToken);

        if (twoFactorAuth is null || twoFactorAuth.EncryptedSecret is null)
        {
            throw new NotFoundException("2FA no está configurado.");
        }

        var secret = System.Text.Encoding.UTF8.GetString(
            _encryptionService.Decrypt(twoFactorAuth.EncryptedSecret));

        var isValid = _totpService.ValidateCode(secret, request.Code);

        if (!isValid)
        {
            throw new UnauthorizedException("Código 2FA inválido.");
        }

        if (!twoFactorAuth.IsActive)
        {
            twoFactorAuth.Enable();
            _twoFactorAuthRepository.Update(twoFactorAuth);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
