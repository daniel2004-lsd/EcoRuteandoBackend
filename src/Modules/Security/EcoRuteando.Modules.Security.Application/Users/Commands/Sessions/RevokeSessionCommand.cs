using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed record RevokeSessionCommand(
    Guid UserId,
    Guid SessionId) : IRequest<Unit>;
