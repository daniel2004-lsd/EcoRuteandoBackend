namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface IOAuthProvider
{
    string ProviderName { get; }

    Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken);
}

public sealed class OAuthUserInfo
{
    public string ExternalId { get; init; } = default!;
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? PhotoUrl { get; init; }
}
