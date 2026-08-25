using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LogoutUser;

public sealed class LogoutUserCommandHandler
    : IRequestHandler<LogoutUserCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public LogoutUserCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenService refreshTokenService,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task Handle(
     LogoutUserCommand request,
     CancellationToken cancellationToken)
    {
        var refreshTokenHash =
            _refreshTokenService.ComputeHash(request.RefreshToken);

        var refreshToken = await _refreshTokenRepository.GetByHashAsync(
            refreshTokenHash,
            cancellationToken);

        if (refreshToken is null)
        {
            throw new UnauthorizedException("Refresh token inválido.");
        }

        if (!refreshToken.IsActive)
        {
            throw new UnauthorizedException("El refresh token ya fue revocado o expiró.");
        }

        refreshToken.Revoke(
            revokedByIp: null,
            replacedByRefreshTokenHash: null);

        // Revoke session
        var session = await _sessionRepository.GetByRefreshTokenHashAsync(
            refreshTokenHash, cancellationToken);

        if (session is not null)
        {
            session.Revoke();
            _sessionRepository.Update(session);
        }

        await _refreshTokenRepository.UpdateAsync(
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            refreshToken.UserId,
            "user.logout",
            entityName: "users",
            entityId: refreshToken.UserId.ToString(),
            cancellationToken: cancellationToken);
    }
}