namespace EcoRuteando.Modules.Security.Application.SecurityPolicies.Queries.GetSecurityPolicy;

public sealed record SecurityPolicyResponse(
    int MinPasswordLength,
    bool RequireUppercase,
    bool RequireNumbers,
    bool RequireSpecialChars,
    int PasswordExpirationDays,
    int MaxFailedAttempts,
    int LockoutTimeMinutes,
    int MaxActiveSessions,
    DateTime UpdatedAt);
