namespace EcoRuteando.Modules.Security.Presentation.Contracts.SecurityPolicy;

public sealed record UpdateSecurityPolicyRequest(
    int MinPasswordLength,
    bool RequireUppercase,
    bool RequireNumbers,
    bool RequireSpecialChars,
    int PasswordExpirationDays,
    int MaxFailedAttempts,
    int LockoutTimeMinutes,
    int MaxActiveSessions);
