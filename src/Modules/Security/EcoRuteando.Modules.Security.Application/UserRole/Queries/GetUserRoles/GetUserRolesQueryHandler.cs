using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.UserRoles.Queries.GetUserRoles;

public sealed class GetUserRolesQueryHandler
    : IRequestHandler<GetUserRolesQuery, IReadOnlyList<UserRoleResponse>>
{
    private readonly IUserRoleRepository _userRoleRepository;

    public GetUserRolesQueryHandler(
        IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task<IReadOnlyList<UserRoleResponse>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        return userRoles
            .Select(ur => new UserRoleResponse(
                ur.RoleId,
                ur.Role.Name))
            .ToList();
    }
}