using EcoRuteando.Shared.BaseClasses;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public enum OAuthProvider
{
    Google = 0,
    Facebook = 1,
    Github = 2,
    Apple = 3
}

public sealed class OAuthAccount : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public OAuthProvider Provider { get; private set; }
    public string ExternalId { get; private set; } = default!;
    public byte[]? EncryptedAccessToken { get; private set; }
    public byte[]? EncryptedRefreshToken { get; private set; }
    public string? TokenScope { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }
    public string? OAuthEmail { get; private set; }
    public string? OAuthName { get; private set; }
    public string? OAuthPhotoUrl { get; private set; }

    public User User { get; private set; } = default!;

    private OAuthAccount() { }

    public OAuthAccount(
        Guid userId,
        OAuthProvider provider,
        string externalId,
        string? oauthEmail,
        string? oauthName,
        string? oauthPhotoUrl)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Provider = provider;
        ExternalId = externalId;
        OAuthEmail = oauthEmail;
        OAuthName = oauthName;
        OAuthPhotoUrl = oauthPhotoUrl;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTokens(
        byte[]? encryptedAccessToken,
        byte[]? encryptedRefreshToken,
        string? tokenScope,
        DateTime? tokenExpiresAt)
    {
        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        TokenScope = tokenScope;
        TokenExpiresAt = tokenExpiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
