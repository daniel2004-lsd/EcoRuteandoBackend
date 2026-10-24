using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed class RevokeSessionCommandHandler
    : IRequestHandler<RevokeSessionCommand, Unit>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public RevokeSessionCommandHandler(
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        RevokeSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(
            request.SessionId, cancellationToken);

        if (session is null || session.UserId != request.UserId)
        {
            throw new NotFoundException("Sesión no encontrada.");
        }

        session.Revoke();
        _sessionRepository.Update(session);

        // Revocación en cascada del refresh token asociado a esta sesión
        var refreshToken = await _refreshTokenRepository.GetByHashAsync(
            session.RefreshTokenHash, cancellationToken);

        if (refreshToken is not null && refreshToken.IsActive)
        {
            refreshToken.Revoke(revokedByIp: null, replacedByRefreshTokenHash: null);
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
