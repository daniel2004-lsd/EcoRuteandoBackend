using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed record GetActiveSessionsQuery(
    Guid UserId) : IRequest<IReadOnlyList<SessionResponse>>;

public sealed record SessionResponse(
    Guid Id,
    string? Device,
    string? SourceIp,
    string? UserAgent,
    DateTime LastUsedAt,
    DateTime ExpiresAt);
