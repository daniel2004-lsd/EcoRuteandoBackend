using MediatR;

namespace EcoRuteando.Modules.Security.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(int Take = 200)
    : IRequest<IReadOnlyList<AuditLogResponse>>;
