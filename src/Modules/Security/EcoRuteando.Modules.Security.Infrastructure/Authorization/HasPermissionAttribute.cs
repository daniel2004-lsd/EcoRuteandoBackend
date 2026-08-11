using Microsoft.AspNetCore.Authorization;

namespace EcoRuteando.Modules.Security.Infrastructure.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission";

    public HasPermissionAttribute(string permission)
    {
        Policy = $"{PolicyPrefix}:{permission}";
    }
}