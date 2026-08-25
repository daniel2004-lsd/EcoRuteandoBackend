using System.Net;
using EcoRuteando.Shared.BaseClasses;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class EmailVerification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public IPAddress? RequestIp { get; private set; }

    public User User { get; private set; } = default!;

    private EmailVerification() { }

    public EmailVerification(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        IPAddress? requestIp)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        RequestIp = requestIp;
        IsVerified = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsVerified()
    {
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
