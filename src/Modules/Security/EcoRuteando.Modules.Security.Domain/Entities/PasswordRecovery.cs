using EcoRuteando.Shared.BaseClasses;
using System.Net;
namespace EcoRuteando.Modules.Security.Domain.Entities;

public class PasswordRecovery : Entity<Guid>
{
   

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = default!;

    public DateTime ExpiresAt { get; private set; }

    public bool IsUsed { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public IPAddress? RequestIp { get; private set; }

    public IPAddress? UsageIp { get; private set; }



    // Relación
    public User User { get; private set; } = default!;

    private PasswordRecovery()
    {
    }

    public PasswordRecovery(
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
        IsUsed = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsUsed(IPAddress? usageIp)
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UsageIp = usageIp;
    }
}