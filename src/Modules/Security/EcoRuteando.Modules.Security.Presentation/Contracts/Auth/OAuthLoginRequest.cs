namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth;

public sealed record OAuthLoginRequest(
    string Provider,
    string AccessToken);
