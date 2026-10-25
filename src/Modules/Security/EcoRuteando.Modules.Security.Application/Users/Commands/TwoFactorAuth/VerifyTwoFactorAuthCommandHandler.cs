using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed class VerifyTwoFactorAuthCommandHandler
    : IRequestHandler<VerifyTwoFactorAuthCommand, VerifyTwoFactorAuthResponse>
{
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public VerifyTwoFactorAuthCommandHandler(
        ITotpService totpService,
        IEncryptionService encryptionService,
        ITwoFactorAuthRepository twoFactorAuthRepository,
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ISessionRepository sessionRepository,
        ITrustedDeviceService trustedDeviceService,
        ISecurityUnitOfWork unitOfWork)
    {
        _totpService = totpService;
        _encryptionService = encryptionService;
        _twoFactorAuthRepository = twoFactorAuthRepository;
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _trustedDeviceService = trustedDeviceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyTwoFactorAuthResponse> Handle(
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
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenService.ComputeHash(refreshToken);

        var refreshTokenEntity = new EcoRuteando.Modules.Security.Domain.Entities.RefreshToken(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(7),
            null);

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        var session = new Session(
            user.Id,
            refreshTokenHash,
            null,
            null,
            null,
            DateTime.UtcNow.AddDays(7));

        await _sessionRepository.AddAsync(session, cancellationToken);

        var roleName = user.PrimaryRole?.Name ?? "user";
        var accessToken = _jwtProvider.GenerateToken(
            user.Id,
            user.Email,
            roleName,
            session.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string? trustedDeviceToken = null;
        if (request.RememberDevice)
        {
            trustedDeviceToken = await _trustedDeviceService.GenerateTrustedDeviceTokenAsync(
                user.Id, 30, cancellationToken);
        }

        return new VerifyTwoFactorAuthResponse(
            "2FA verificado correctamente.",
            accessToken,
            refreshToken,
            trustedDeviceToken);
    }
}
