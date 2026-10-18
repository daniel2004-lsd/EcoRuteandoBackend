namespace EcoRuteando.Modules.Security.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string Action,
    string? EntityName,
    string? EntityId,
    string? SourceIp,
    string? UserAgent,
    DateTime CreatedAt);
