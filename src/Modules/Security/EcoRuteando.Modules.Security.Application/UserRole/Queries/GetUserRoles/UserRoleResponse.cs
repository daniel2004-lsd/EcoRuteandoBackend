namespace EcoRuteando.Modules.Security.Application.UserRoles.Queries.GetUserRoles;

public sealed record UserRoleResponse(
    int RoleId,
    string RoleName);