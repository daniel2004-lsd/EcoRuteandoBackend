namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth;

public sealed record VerifyEmailRequest(
    string Code);
