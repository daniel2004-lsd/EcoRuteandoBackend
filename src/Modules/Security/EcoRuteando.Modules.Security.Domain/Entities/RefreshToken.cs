using EcoRuteando.Shared.BaseClasses;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? createdByIp)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

 

    public DateTime? RevokedAt { get; private set; }

    public string? ReplacedByRefreshTokenHash { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked =>
        RevokedAt.HasValue;

    public bool IsActive =>
        !IsExpired && !IsRevoked;

    public void Revoke(
        string? revokedByIp,
        string? replacedByRefreshTokenHash)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByRefreshTokenHash = replacedByRefreshTokenHash;
    }
}