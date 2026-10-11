namespace EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;

public sealed record VerifyTwoFactorAuthResponse(
    string Message,
    string AccessToken,
    string RefreshToken,
    string? TrustedDeviceToken = null);
