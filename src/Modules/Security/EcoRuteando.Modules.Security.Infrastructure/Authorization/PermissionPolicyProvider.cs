using EcoRuteando.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Security.Infrastructure.Authorization;

public sealed class PermissionPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith($"{HasPermissionAttribute.PolicyPrefix}:"))
        {
            var permission = policyName.Substring(
                $"{HasPermissionAttribute.PolicyPrefix}:".Length);

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return policy;
        }

        return await base.GetPolicyAsync(policyName);
    }
}