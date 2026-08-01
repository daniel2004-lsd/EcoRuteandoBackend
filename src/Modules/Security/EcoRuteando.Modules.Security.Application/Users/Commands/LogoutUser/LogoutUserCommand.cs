using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LogoutUser;

public sealed record LogoutUserCommand(
    string RefreshToken)
    : IRequest;