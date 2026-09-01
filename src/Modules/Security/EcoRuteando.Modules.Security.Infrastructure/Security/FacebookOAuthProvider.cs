using System.Net.Http.Json;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class FacebookOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;

    public string ProviderName => "facebook";

    public FacebookOAuthProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken)
    {
        var response = await _httpClient.GetAsync(
            $"https://graph.facebook.com/me?fields=id,email,name,picture.type(large)&access_token={accessToken}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var userInfo = await response.Content.ReadFromJsonAsync<FacebookUserInfo>();

        if (userInfo is null)
        {
            return null;
        }

        return new OAuthUserInfo
        {
            ExternalId = userInfo.Id,
            Email = userInfo.Email,
            Name = userInfo.Name,
            PhotoUrl = userInfo.Picture?.Data?.Url
        };
    }

    private sealed class FacebookUserInfo
    {
        public string Id { get; set; } = default!;
        public string? Email { get; set; }
        public string? Name { get; set; }
        public FacebookPicture? Picture { get; set; }
    }

    private sealed class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private sealed class FacebookPictureData
    {
        public string? Url { get; set; }
    }
}
