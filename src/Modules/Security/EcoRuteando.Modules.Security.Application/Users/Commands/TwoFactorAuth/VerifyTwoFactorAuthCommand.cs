using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed record VerifyTwoFactorAuthCommand(
    Guid UserId,
    string Code) : IRequest<bool>;
