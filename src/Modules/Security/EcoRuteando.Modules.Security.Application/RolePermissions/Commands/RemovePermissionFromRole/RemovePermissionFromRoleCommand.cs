using MediatR;

namespace EcoRuteando.Modules.Security.Application.RolePermissions.Commands.RemovePermissionFromRole;

public sealed record RemovePermissionFromRoleCommand(
    int RoleId,
    int PermissionId)
    : IRequest;