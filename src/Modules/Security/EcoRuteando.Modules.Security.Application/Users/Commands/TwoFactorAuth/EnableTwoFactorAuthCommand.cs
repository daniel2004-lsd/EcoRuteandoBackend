using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed record EnableTwoFactorAuthCommand(
    Guid UserId) : IRequest<TwoFactorSetupResponse>;

public sealed record TwoFactorSetupResponse(
    string Secret,
    string QrCodeUri,
    string[] RecoveryCodes);
