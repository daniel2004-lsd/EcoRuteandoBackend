using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery(
    Guid RoleId)
    : IRequest<IReadOnlyList<RolePermissionResponse>>;