
using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;
using System.Linq;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserResponse>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .Select(user => new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PrimaryRole?.Name ?? string.Empty))
            .ToList();
    }
}

