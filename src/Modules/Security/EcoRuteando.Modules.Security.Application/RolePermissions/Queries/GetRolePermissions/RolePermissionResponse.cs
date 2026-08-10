namespace EcoRuteando.Modules.Security.Application.RolePermissions.Queries.GetRolePermissions;

public sealed record RolePermissionResponse(
    Guid PermissionId,
    string PermissionName);