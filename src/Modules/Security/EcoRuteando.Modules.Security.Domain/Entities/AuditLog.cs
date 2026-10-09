using EcoRuteando.Shared.BaseClasses;
using System.Net;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class AuditLog : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string? EntityName { get; private set; }
    public string? EntityId { get; private set; }
    public string? BeforeData { get; private set; }
    public string? AfterData { get; private set; }
    public IPAddress? SourceIp { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditLog() { }

    public AuditLog(
        Guid? userId,
        string action,
        string? entityName,
        string? entityId,
        string? beforeData,
        string? afterData,
        IPAddress? sourceIp,
        string? userAgent)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        BeforeData = beforeData;
        AfterData = afterData;
        SourceIp = sourceIp;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }
}
