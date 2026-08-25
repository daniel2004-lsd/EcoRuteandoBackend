using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed record DisableTwoFactorAuthCommand(
    Guid UserId,
    string Code) : IRequest<bool>;
