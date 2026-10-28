using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed class RevokeAllSessionsCommandHandler
    : IRequestHandler<RevokeAllSessionsCommand, int>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITrustedDeviceService _trustedDeviceService;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public RevokeAllSessionsCommandHandler(
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITrustedDeviceService trustedDeviceService,
        ISecurityUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _trustedDeviceService = trustedDeviceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(
        RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetActiveByUserIdAsync(
            request.UserId, cancellationToken);

        var count = 0;
        foreach (var session in sessions)
        {
            session.Revoke();
            _sessionRepository.Update(session);
            count++;
        }

        // Invalidar todos los refresh tokens activos del usuario
        var refreshTokens = await _refreshTokenRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        foreach (var rt in refreshTokens.Where(t => t.IsActive))
        {
            rt.Revoke(revokedByIp: null, replacedByRefreshTokenHash: null);
            await _refreshTokenRepository.UpdateAsync(rt, cancellationToken);
        }

        // Invalidar todos los dispositivos confiables del usuario
        await _trustedDeviceService.RevokeAllForUserAsync(request.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return count;
    }
}
