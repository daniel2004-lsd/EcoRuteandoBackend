using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string? UsageIp = null,
    string? Email = null
) : IRequest;