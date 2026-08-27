namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    bool RequiresTwoFactor = false,
    string? TwoFactorToken = null,
    bool RequiresEmailVerification = false);