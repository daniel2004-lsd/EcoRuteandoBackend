using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginOAuth;

public sealed class LoginWithOAuthCommandHandler
    : IRequestHandler<LoginWithOAuthCommand, LoginResponse>
{
    private readonly IEnumerable<IOAuthProvider> _oauthProviders;
    private readonly IOAuthAccountRepository _oauthAccountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public LoginWithOAuthCommandHandler(
        IEnumerable<IOAuthProvider> oauthProviders,
        IOAuthAccountRepository oauthAccountRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtProvider jwtProvider,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ISessionRepository sessionRepository,
        ISecurityPolicyRepository securityPolicyRepository,
        ISecurityUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _oauthProviders = oauthProviders;
        _oauthAccountRepository = oauthAccountRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtProvider = jwtProvider;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _securityPolicyRepository = securityPolicyRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<LoginResponse> Handle(
        LoginWithOAuthCommand request,
        CancellationToken cancellationToken)
    {
        var provider = _oauthProviders
            .FirstOrDefault(p => p.ProviderName.Equals(
                request.Provider, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            throw new NotFoundException(
                $"El proveedor OAuth '{request.Provider}' no está soportado.");
        }

        if (!Enum.TryParse<OAuthProvider>(
                request.Provider, ignoreCase: true, out var providerType))
        {
            throw new NotFoundException(
                $"El proveedor OAuth '{request.Provider}' no está soportado.");
        }

        var oauthUserInfo = await provider.GetUserInfoAsync(request.AccessToken);

        if (oauthUserInfo is null)
        {
            throw new UnauthorizedException(
                "No se pudo obtener la información del usuario desde el proveedor OAuth.");
        }

        var existingAccount =
            await _oauthAccountRepository.GetByProviderAndExternalIdAsync(
                providerType,
                oauthUserInfo.ExternalId,
                cancellationToken);

        if (existingAccount is not null)
        {
            return await GenerateTokensForUser(
                existingAccount.User, cancellationToken);
        }

        User user;

        if (!string.IsNullOrWhiteSpace(oauthUserInfo.Email))
        {
            var existingUser = await _userRepository.GetByEmailAsync(
                oauthUserInfo.Email,
                cancellationToken);

            if (existingUser is not null)
            {
                user = existingUser;
            }
            else
            {
                user = await CreateNewUser(
                    oauthUserInfo, cancellationToken);
            }
        }
        else
        {
            user = await CreateNewUser(oauthUserInfo, cancellationToken);
        }

        var oauthAccount = new OAuthAccount(
            user.Id,
            providerType,
            oauthUserInfo.ExternalId,
            oauthUserInfo.Email,
            oauthUserInfo.Name,
            oauthUserInfo.PhotoUrl);

        await _oauthAccountRepository.AddAsync(
            oauthAccount, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GenerateTokensForUser(user, cancellationToken);
    }

    private async Task<User> CreateNewUser(
        OAuthUserInfo oauthUserInfo,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByNameAsync(
            "User", cancellationToken);

        if (role is null)
        {
            throw new NotFoundException("El rol 'User' no existe.");
        }

        var user = new User(
            oauthUserInfo.Name ?? "Usuario",
            null,
            oauthUserInfo.Email ?? $"{oauthUserInfo.ExternalId}@oauth.placeholder",
            null,
            null);

        user.AcceptTerms();
        user.VerifyEmail();
        user.AssignPrimaryRole(role);

        await _userRepository.AddAsync(user, cancellationToken);

        user.UserRoles.Add(new UserRole(user.Id, role.Id));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user;
    }

    private async Task<LoginResponse> GenerateTokensForUser(
        User user,
        CancellationToken cancellationToken)
    {
        if (user.PrimaryRole is null)
        {
            throw new DomainException("El usuario no tiene un rol asignado.");
        }

        var accessToken = _jwtProvider.GenerateToken(
            user.Id,
            user.Email,
            user.PrimaryRole.Name);

        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenService.ComputeHash(refreshToken);

        var refreshTokenEntity = new EcoRuteando.Modules.Security.Domain.Entities.RefreshToken(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(7),
            null);

        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity, cancellationToken);

        var policy = await _securityPolicyRepository.GetAsync(cancellationToken);
        var maxActiveSessions = policy?.MaxActiveSessions ?? 0;

        if (maxActiveSessions > 0)
        {
            var activeSessions = await _sessionRepository.GetActiveByUserIdAsync(
                user.Id, cancellationToken);

            var excessCount = activeSessions.Count - maxActiveSessions + 1;

            foreach (var session in activeSessions
                .OrderBy(s => s.CreatedAt)
                .Take(Math.Max(0, excessCount)))
            {
                session.Revoke();
                _sessionRepository.Update(session);
            }
        }

        var newSession = new EcoRuteando.Modules.Security.Domain.Entities.Session(
            user.Id,
            refreshTokenHash,
            null,
            null,
            null,
            DateTime.UtcNow.AddDays(7));

        await _sessionRepository.AddAsync(newSession, cancellationToken);

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            user.Id,
            "user.oauth_login",
            entityName: "users",
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return new LoginResponse(accessToken, refreshToken);
    }
}
