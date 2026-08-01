namespace EcoRuteando.Modules.Security.Application.RolePermissions.Queries.GetRolePermissions;

public sealed record RolePermissionResponse(
    int PermissionId,
    string PermissionName);