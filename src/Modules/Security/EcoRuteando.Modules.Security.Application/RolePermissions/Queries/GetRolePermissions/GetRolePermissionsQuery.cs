using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery(
    int RoleId)
    : IRequest<IReadOnlyList<RolePermissionResponse>>;