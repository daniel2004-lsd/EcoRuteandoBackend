using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

        return new CurrentUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PrimaryRole.Name);
    }
}