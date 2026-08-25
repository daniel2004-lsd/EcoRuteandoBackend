using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;

public sealed class GetSecurityPolicyQueryHandler
    : IRequestHandler<GetSecurityPolicyQuery, SecurityPolicyResponse>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;

    public GetSecurityPolicyQueryHandler(
        ISecurityPolicyRepository securityPolicyRepository)
    {
        _securityPolicyRepository = securityPolicyRepository;
    }

    public async Task<SecurityPolicyResponse> Handle(
        GetSecurityPolicyQuery request,
        CancellationToken cancellationToken)
    {
        var policy = await _securityPolicyRepository.GetAsync(cancellationToken);

        // Defaults used by the application when no policy row exists.
        policy ??= new SecurityPolicy(
            minPasswordLength: 8,
            requireUppercase: true,
            requireNumbers: true,
            requireSpecialChars: true,
            passwordExpirationDays: 0,
            maxFailedAttempts: 5,
            lockoutTimeMinutes: 30,
            maxActiveSessions: 0);

        return new SecurityPolicyResponse(
            policy.MinPasswordLength,
            policy.RequireUppercase,
            policy.RequireNumbers,
            policy.RequireSpecialChars,
            policy.PasswordExpirationDays,
            policy.MaxFailedAttempts,
            policy.LockoutTimeMinutes,
            policy.MaxActiveSessions,
            policy.UpdatedAt);
    }
}
