namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth;

public sealed record LogoutRequest(
    string RefreshToken);