using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;

public sealed record SendVerificationEmailCommand(
    Guid? UserId,
    string? Email,
    string? RequestIp)
    : IRequest;
