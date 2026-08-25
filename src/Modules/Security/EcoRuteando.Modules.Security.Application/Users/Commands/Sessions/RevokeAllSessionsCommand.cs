using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed record RevokeAllSessionsCommand(
    Guid UserId) : IRequest<int>;
