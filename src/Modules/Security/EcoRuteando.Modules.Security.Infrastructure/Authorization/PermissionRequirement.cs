using Microsoft.AspNetCore.Authorization;

namespace EcoRuteando.Modules.Security.Infrastructure.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException(
                "El permiso es obligatorio.",
                nameof(permission));

        Permission = permission;
    }
}