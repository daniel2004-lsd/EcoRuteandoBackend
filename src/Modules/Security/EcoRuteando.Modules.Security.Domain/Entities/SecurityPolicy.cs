namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class SecurityPolicy
{
    public int Id { get; private set; }
    public int MinPasswordLength { get; private set; }
    public bool RequireUppercase { get; private set; }
    public bool RequireNumbers { get; private set; }
    public bool RequireSpecialChars { get; private set; }
    public int PasswordExpirationDays { get; private set; }
    public int MaxFailedAttempts { get; private set; }
    public int LockoutTimeMinutes { get; private set; }
    public int MaxActiveSessions { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SecurityPolicy() { }

    public SecurityPolicy(
        int minPasswordLength,
        bool requireUppercase,
        bool requireNumbers,
        bool requireSpecialChars,
        int passwordExpirationDays,
        int maxFailedAttempts,
        int lockoutTimeMinutes,
        int maxActiveSessions)
    {
        Id = 1;
        MinPasswordLength = minPasswordLength;
        RequireUppercase = requireUppercase;
        RequireNumbers = requireNumbers;
        RequireSpecialChars = requireSpecialChars;
        PasswordExpirationDays = passwordExpirationDays;
        MaxFailedAttempts = maxFailedAttempts;
        LockoutTimeMinutes = lockoutTimeMinutes;
        MaxActiveSessions = maxActiveSessions;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        int minPasswordLength,
        bool requireUppercase,
        bool requireNumbers,
        bool requireSpecialChars,
        int passwordExpirationDays,
        int maxFailedAttempts,
        int lockoutTimeMinutes,
        int maxActiveSessions)
    {
        MinPasswordLength = minPasswordLength;
        RequireUppercase = requireUppercase;
        RequireNumbers = requireNumbers;
        RequireSpecialChars = requireSpecialChars;
        PasswordExpirationDays = passwordExpirationDays;
        MaxFailedAttempts = maxFailedAttempts;
        LockoutTimeMinutes = lockoutTimeMinutes;
        MaxActiveSessions = maxActiveSessions;
        UpdatedAt = DateTime.UtcNow;
    }
}
