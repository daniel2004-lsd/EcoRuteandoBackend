using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(
    string Code)
    : IRequest;
