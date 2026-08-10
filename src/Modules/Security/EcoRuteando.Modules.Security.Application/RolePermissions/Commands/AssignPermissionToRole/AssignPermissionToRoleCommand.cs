using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Commands.AssignPermissionToRole;

public sealed record AssignPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId)
    : IRequest;