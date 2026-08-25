using EcoRuteando.Shared.BaseClasses;
using System.Net;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class Session : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = default!;
    public IPAddress? SourceIp { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Device { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime LastUsedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = default!;

    private Session() { }

    public Session(
        Guid userId,
        string refreshTokenHash,
        IPAddress? sourceIp,
        string? userAgent,
        string? device,
        DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        SourceIp = sourceIp;
        UserAgent = userAgent;
        Device = device;
        ExpiresAt = expiresAt;
        LastUsedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
