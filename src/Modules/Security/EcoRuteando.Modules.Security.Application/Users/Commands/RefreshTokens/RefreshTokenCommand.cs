using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : IRequest<LoginResponse>;