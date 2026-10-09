using System.Net.Http.Json;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class GoogleOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOptions _options;

    public string ProviderName => "google";

    public GoogleOAuthProvider(HttpClient httpClient, IOptions<GoogleOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken)
    {
        var response = await _httpClient.GetAsync(
            $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfo>();

        if (userInfo is null)
        {
            return null;
        }

        return new OAuthUserInfo
        {
            ExternalId = userInfo.Id,
            Email = userInfo.Email,
            Name = userInfo.Name,
            PhotoUrl = userInfo.Picture
        };
    }

    private sealed class GoogleUserInfo
    {
        public string Id { get; set; } = default!;
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
    }
}
