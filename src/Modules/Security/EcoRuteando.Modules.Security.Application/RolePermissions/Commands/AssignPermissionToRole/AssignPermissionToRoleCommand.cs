using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Commands.AssignPermissionToRole;

public sealed record AssignPermissionToRoleCommand(
    int RoleId,
    int PermissionId)
    : IRequest;