using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommandHandler
    : IRequestHandler<ChangeMyPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public ChangeMyPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITrustedDeviceService trustedDeviceService,
        ISecurityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _trustedDeviceService = trustedDeviceService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ChangeMyPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new DomainException(
                "Esta cuenta está vinculada a Google. La autenticación y contraseña son gestionadas directamente por Google.");
        }

        if (!_passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash))
        {
            throw new DomainException(
                "La contraseña actual es incorrecta.");
        }

        user.ChangePassword(
            _passwordHasher.Hash(request.NewPassword));

        await _userRepository.UpdateAsync(
            user,
            cancellationToken);

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
                rt.Revoke(revokedByIp: null, replacedByRefreshTokenHash: null);
                await _refreshTokenRepository.UpdateAsync(rt, cancellationToken);
            }
        }

        // Invalidar dispositivos confiables del usuario (H-23 / H-24)
        if (_trustedDeviceService is not null)
        {
            await _trustedDeviceService.RevokeAllForUserAsync(user.Id, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
