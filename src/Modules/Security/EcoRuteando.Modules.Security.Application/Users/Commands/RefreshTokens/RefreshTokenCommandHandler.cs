using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtProvider _jwtProvider;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenService refreshTokenService,
        IJwtProvider jwtProvider,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenService = refreshTokenService;
        _jwtProvider = jwtProvider;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
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
            throw new UnauthorizedException("Refresh token expirado o revocado.");
        }

        var user = refreshToken.User;

        if (user.PrimaryRole is null)
        {
            throw new DomainException("El usuario no tiene un rol asignado.");
        }

        var accessToken = _jwtProvider.GenerateToken(
            user.Id,
            user.Email,
            user.PrimaryRole.Name);

        var newRefreshToken =
            _refreshTokenService.GenerateRefreshToken();

        var newRefreshTokenHash =
            _refreshTokenService.ComputeHash(newRefreshToken);


        refreshToken.Revoke(
        revokedByIp: null,
        replacedByRefreshTokenHash: newRefreshTokenHash);

        // Update session
        var session = await _sessionRepository.GetByRefreshTokenHashAsync(
            refreshTokenHash, cancellationToken);

        if (session is not null)
        {
            session.Revoke();
            _sessionRepository.Update(session);
        }

        // Crear el nuevo refresh token
        var refreshTokenEntity = new EcoRuteando.Modules.Security.Domain.Entities.RefreshToken(
            user.Id,
            newRefreshTokenHash,
            DateTime.UtcNow.AddDays(7),
            null);

        // Create new session
        var newSession = new EcoRuteando.Modules.Security.Domain.Entities.Session(
            user.Id,
            newRefreshTokenHash,
            session?.SourceIp,
            session?.UserAgent,
            session?.Device,
            DateTime.UtcNow.AddDays(7));

        await _sessionRepository.AddAsync(newSession, cancellationToken);

        // Guardarlo
        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity,
            cancellationToken);

        // Actualizar el token anterior
        await _refreshTokenRepository.UpdateAsync(
            refreshToken,
            cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Devolver los nuevos tokens
        return new LoginResponse(
            accessToken,
            newRefreshToken);
    }
}