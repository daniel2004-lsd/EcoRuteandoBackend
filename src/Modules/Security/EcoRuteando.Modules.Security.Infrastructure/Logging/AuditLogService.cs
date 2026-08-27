using System.Net;
using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;

namespace EcoRuteando.Modules.Security.Infrastructure.Logging;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(
        Guid? userId,
        string action,
        string? entityName = null,
        string? entityId = null,
        string? beforeData = null,
        string? afterData = null,
        string? sourceIp = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        IPAddress.TryParse(sourceIp, out var sourceIpAddress);

        var auditLog = new AuditLog(
            userId,
            action,
            entityName,
            entityId,
            beforeData,
            afterData,
            sourceIpAddress,
            userAgent);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
