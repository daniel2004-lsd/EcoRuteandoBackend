using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository,
        ITwoFactorAuthRepository twoFactorAuthRepository)
    {
        _userRepository = userRepository;
        _twoFactorAuthRepository = twoFactorAuthRepository;
    }

    public async Task<CurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        if (user.PrimaryRole is null)
        {
            throw new DomainException("El usuario no tiene un rol asignado.");
        }

        var twoFactor = await _twoFactorAuthRepository.GetByUserIdAndMethodAsync(
            request.UserId,
            TwoFactorMethod.TOTP,
            cancellationToken);

        bool isTwoFactorEnabled = twoFactor is { IsActive: true };
        bool hasPassword = !string.IsNullOrWhiteSpace(user.PasswordHash);
        string authProvider = hasPassword ? "local" : "google";

        return new CurrentUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.PrimaryRole.Name,
            hasPassword,
            isTwoFactorEnabled,
            authProvider);
    }
}