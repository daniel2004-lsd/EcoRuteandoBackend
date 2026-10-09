namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginOAuth;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken);
