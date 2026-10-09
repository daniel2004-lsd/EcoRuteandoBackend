namespace EcoRuteando.Modules.Security.Application.Abstractions.Logging;

public interface IAuditLogService
{
    Task LogAsync(
        Guid? userId,
        string action,
        string? entityName = null,
        string? entityId = null,
        string? beforeData = null,
        string? afterData = null,
        string? sourceIp = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
