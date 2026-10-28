using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public sealed class OAuthProvidersSecurityTests
{
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    [Fact]
    public async Task GoogleOAuthProvider_WhenValidToken_SendsAuthorizationBearerHeaderAndNoQueryString()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var token = "google_sec_test_token_xyz_123";

        var handler = new MockHttpMessageHandler(req =>
        {
            capturedRequest = req;
            var json = JsonSerializer.Serialize(new
            {
                id = "google_user_12345",
                email = "user@gmail.com",
                name = "Google Test User",
                picture = "https://lh3.googleusercontent.com/photo.jpg"
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new GoogleOptions { ClientId = "test-client-id" });
        var provider = new GoogleOAuthProvider(httpClient, options);

        // Act
        var result = await provider.GetUserInfoAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("google_user_12345", result.ExternalId);
        Assert.Equal("user@gmail.com", result.Email);
        Assert.Equal("Google Test User", result.Name);
        Assert.Equal("https://lh3.googleusercontent.com/photo.jpg", result.PhotoUrl);

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest.Method);
        Assert.Equal("https://www.googleapis.com/oauth2/v2/userinfo", capturedRequest.RequestUri?.GetLeftPart(UriPartial.Path));

        // RFC 6750 Section 2.3 verification: Access token MUST NOT be exposed in query string
        Assert.False(capturedRequest.RequestUri?.Query.Contains("access_token"), "OAuth access token must not be in query string");

        // RFC 6750 Section 2.1 verification: Access token MUST be sent via Authorization Bearer header
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(token, capturedRequest.Headers.Authorization.Parameter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GoogleOAuthProvider_WhenTokenIsEmptyOrWhitespace_DoesNotSendRequestAndReturnsNull(string? invalidToken)
    {
        // Arrange
        var requestSent = false;
        var handler = new MockHttpMessageHandler(req =>
        {
            requestSent = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new GoogleOptions());
        var provider = new GoogleOAuthProvider(httpClient, options);

        // Act
        var result = await provider.GetUserInfoAsync(invalidToken!);

        // Assert
        Assert.Null(result);
        Assert.False(requestSent, "HTTP request must not be sent for empty or invalid token");
    }

    [Fact]
    public async Task GoogleOAuthProvider_WhenProviderReturnsUnauthorized_ReturnsNull()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new GoogleOptions());
        var provider = new GoogleOAuthProvider(httpClient, options);

        // Act
        var result = await provider.GetUserInfoAsync("expired_or_invalid_token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FacebookOAuthProvider_WhenValidToken_SendsAuthorizationBearerHeaderAndNoQueryString()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var token = "facebook_sec_test_token_abc_789";

        var handler = new MockHttpMessageHandler(req =>
        {
            capturedRequest = req;
            var json = JsonSerializer.Serialize(new
            {
                id = "fb_user_67890",
                email = "user@facebook.com",
                name = "Facebook Test User",
                picture = new
                {
                    data = new
                    {
                        url = "https://graph.facebook.com/photo.jpg"
                    }
                }
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var provider = new FacebookOAuthProvider(httpClient);

        // Act
        var result = await provider.GetUserInfoAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fb_user_67890", result.ExternalId);
        Assert.Equal("user@facebook.com", result.Email);
        Assert.Equal("Facebook Test User", result.Name);
        Assert.Equal("https://graph.facebook.com/photo.jpg", result.PhotoUrl);

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest.Method);
        Assert.Equal("https://graph.facebook.com/me", capturedRequest.RequestUri?.GetLeftPart(UriPartial.Path));

        // RFC 6750 Section 2.3 verification: Access token MUST NOT be exposed in query string
        Assert.False(capturedRequest.RequestUri?.Query.Contains("access_token"), "OAuth access token must not be in query string");

        // RFC 6750 Section 2.1 verification: Access token MUST be sent via Authorization Bearer header
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(token, capturedRequest.Headers.Authorization.Parameter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task FacebookOAuthProvider_WhenTokenIsEmptyOrWhitespace_DoesNotSendRequestAndReturnsNull(string? invalidToken)
    {
        // Arrange
        var requestSent = false;
        var handler = new MockHttpMessageHandler(req =>
        {
            requestSent = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = new HttpClient(handler);
        var provider = new FacebookOAuthProvider(httpClient);

        // Act
        var result = await provider.GetUserInfoAsync(invalidToken!);

        // Assert
        Assert.Null(result);
        Assert.False(requestSent, "HTTP request must not be sent for empty or invalid token");
    }

    [Fact]
    public async Task FacebookOAuthProvider_WhenProviderReturnsUnauthorized_ReturnsNull()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(req =>
            new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var httpClient = new HttpClient(handler);
        var provider = new FacebookOAuthProvider(httpClient);

        // Act
        var result = await provider.GetUserInfoAsync("invalid_fb_token");

        // Assert
        Assert.Null(result);
    }
}
