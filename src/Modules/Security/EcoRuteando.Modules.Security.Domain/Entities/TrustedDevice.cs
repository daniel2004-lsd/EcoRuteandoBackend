using EcoRuteando.Shared.BaseClasses;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class TrustedDevice : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string? DeviceName { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = default!;

    private TrustedDevice() { }

    public TrustedDevice(
        Guid id,
        Guid userId,
        DateTime expiresAt,
        string? deviceName = null)
    {
        Id = id;
        UserId = userId;
        ExpiresAt = expiresAt;
        DeviceName = deviceName;
        IsRevoked = false;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
