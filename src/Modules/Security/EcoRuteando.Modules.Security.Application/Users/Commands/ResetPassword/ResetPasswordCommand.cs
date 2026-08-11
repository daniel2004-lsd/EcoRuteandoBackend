using MediatR;
using System.Net;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string? UsageIp
) : IRequest;