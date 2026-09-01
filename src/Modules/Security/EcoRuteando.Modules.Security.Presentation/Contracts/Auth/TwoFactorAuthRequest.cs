namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth;

public sealed record EnableTwoFactorAuthResponse(
    string Secret,
    string QrCodeUri,
    string[] RecoveryCodes);

public sealed record VerifyTwoFactorAuthRequest(
    string Code);

public sealed record DisableTwoFactorAuthRequest(
    string Code);

public sealed record RevokeSessionRequest(
    Guid SessionId);
