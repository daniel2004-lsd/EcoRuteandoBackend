using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed class GetActiveSessionsQueryHandler
    : IRequestHandler<GetActiveSessionsQuery, IReadOnlyList<SessionResponse>>
{
    private readonly ISessionRepository _sessionRepository;

    public GetActiveSessionsQueryHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IReadOnlyList<SessionResponse>> Handle(
        GetActiveSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetActiveByUserIdAsync(
            request.UserId, cancellationToken);

        return sessions.Select(s => new SessionResponse(
            s.Id,
            s.Device,
            s.SourceIp?.ToString(),
            s.UserAgent,
            s.LastUsedAt,
            s.ExpiresAt)).ToList();
    }
}
