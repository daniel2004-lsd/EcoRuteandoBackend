using EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.SecurityPolicies.Commands.UpdateSecurityPolicy;

public sealed record UpdateSecurityPolicyCommand(
    int MinPasswordLength,
    bool RequireUppercase,
    bool RequireNumbers,
    bool RequireSpecialChars,
    int PasswordExpirationDays,
    int MaxFailedAttempts,
    int LockoutTimeMinutes,
    int MaxActiveSessions
) : IRequest<SecurityPolicyResponse>;
