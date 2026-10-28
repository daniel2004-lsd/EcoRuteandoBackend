using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EcoRuteando.IntegrationTests.Infrastructure;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginOAuth;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoRuteando.IntegrationTests.Security;

public sealed class OAuthLoginE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OAuthLoginE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private sealed class StubOAuthProvider : IOAuthProvider
    {
        public string ProviderName { get; }
        private readonly Func<string, Task<OAuthUserInfo?>> _getUserInfo;

        public StubOAuthProvider(string providerName, Func<string, Task<OAuthUserInfo?>> getUserInfo)
        {
            ProviderName = providerName;
            _getUserInfo = getUserInfo;
        }

        public Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken) => _getUserInfo(accessToken);
    }

    [Fact]
    public async Task LoginWithOAuth_WhenUnsupportedProvider_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new OAuthLoginRequest("unsupported_provider", "some_token");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/oauth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LoginWithOAuth_WhenInvalidTokenProvidedToRealProvider_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new OAuthLoginRequest("google", "invalid_dummy_token_xyz");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/oauth/login", request);

        // Assert
        // The real Google OAuth provider rejects the dummy token using Bearer auth and returns null -> Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginWithOAuth_WhenProviderReturnsValidUser_CreatesUserAndReturnsValidJwtSession()
    {
        // Arrange
        var externalId = $"google_id_{Guid.NewGuid():N}";
        var email = $"oauth_{Guid.NewGuid():N}@test.ecoruteando.com";
        var validToken = "valid_test_access_token";

        var stubGoogleProvider = new StubOAuthProvider("google", token =>
        {
            if (token == validToken)
            {
                return Task.FromResult<OAuthUserInfo?>(new OAuthUserInfo
                {
                    ExternalId = externalId,
                    Email = email,
                    Name = "OAuth Integration Tester",
                    PhotoUrl = "https://example.com/avatar.jpg"
                });
            }

            return Task.FromResult<OAuthUserInfo?>(null);
        });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var existingProviders = services.Where(d => d.ServiceType == typeof(IOAuthProvider)).ToList();
                foreach (var p in existingProviders)
                {
                    services.Remove(p);
                }

                services.AddSingleton<IOAuthProvider>(stubGoogleProvider);
            });
        }).CreateClient();

        var request = new OAuthLoginRequest("google", validToken);

        // Act
        var loginResponse = await client.PostAsJsonAsync("/api/auth/oauth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrWhiteSpace(loginBody.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginBody.RefreshToken));

        // Verify the emitted JWT allows calling authenticated endpoints
        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);

        var meResponse = await client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meBody = await meResponse.Content.ReadFromJsonAsync<CurrentUserResponse>(JsonOptions);
        Assert.NotNull(meBody);
        Assert.Equal(email, meBody.Email);
    }
}
