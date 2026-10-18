using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.AuditLogs.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;

    public GetAuditLogsQueryHandler(
        IAuditLogRepository auditLogRepository,
        IUserRepository userRepository)
    {
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AuditLogResponse>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetAllAsync(
            request.Take,
            cancellationToken);

        var userIds = logs
            .Where(l => l.UserId.HasValue)
            .Select(l => l.UserId!.Value)
            .Distinct()
            .ToList();

        var emails = new Dictionary<Guid, string>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
            {
                emails[userId] = user.Email;
            }
        }

        return logs
            .Select(l => new AuditLogResponse(
                l.Id,
                l.UserId,
                l.UserId.HasValue && emails.TryGetValue(l.UserId.Value, out var email)
                    ? email
                    : null,
                l.Action,
                l.EntityName,
                l.EntityId,
                l.SourceIp?.ToString(),
                l.UserAgent,
                l.CreatedAt))
            .ToList();
    }
}
