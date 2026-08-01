using Microsoft.AspNetCore.Authorization;

namespace EcoRuteando.Shared.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission";

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"{PolicyPrefix}:{permission}";
    }

    public string Permission { get; }
}