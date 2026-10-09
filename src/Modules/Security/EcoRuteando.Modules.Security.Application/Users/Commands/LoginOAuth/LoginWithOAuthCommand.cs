using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginOAuth;

public sealed record LoginWithOAuthCommand(
    string Provider,
    string AccessToken) : IRequest<LoginResponse>;
