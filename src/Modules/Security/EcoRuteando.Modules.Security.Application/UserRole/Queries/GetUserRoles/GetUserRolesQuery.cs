using MediatR;

namespace EcoRuteando.Modules.Security.Application.UserRoles.Queries.GetUserRoles;

public sealed record GetUserRolesQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<UserRoleResponse>>;