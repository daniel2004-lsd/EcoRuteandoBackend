using System.Security.Claims;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
namespace EcoRuteando.Modules.Security.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{

    private readonly SecurityDbContext _dbContext;

    public PermissionAuthorizationHandler(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Reject tokens with two_factor_pending role — user must complete 2FA first
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value == "two_factor_pending")
        {
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            return;
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

     var hasPermission = await _dbContext.UserRoles
        .Where(ur => ur.UserId == userId)
        .SelectMany(ur => ur.Role.RolePermissions)
        .AnyAsync(rp => rp.Permission.Name == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}