using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email,
    string? RequestIp
) : IRequest;