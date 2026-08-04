namespace EcoRuteando.Modules.Security.Application.UserRoles.Queries.GetUserRoles;

public sealed record UserRoleResponse(
    Guid RoleId,
    string RoleName);