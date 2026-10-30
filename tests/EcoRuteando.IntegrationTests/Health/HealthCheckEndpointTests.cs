using System.Net;
using System.Net.Http.Headers;
using EcoRuteando.IntegrationTests.Infrastructure;
using Xunit;

namespace EcoRuteando.IntegrationTests.Health;

public sealed class HealthCheckEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public HealthCheckEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOkStatus200_WhenApplicationIsRunning()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ShouldBePublic_WithoutRequiringAuthenticationOrAuthorization()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body.Trim());
    }

    [Fact]
    public async Task GetHealth_ShouldNotExposeSensitiveInformationOrInternalConfiguration()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal("Healthy", body.Trim());
        Assert.DoesNotContain("Host=", body);
        Assert.DoesNotContain("Password=", body);
        Assert.DoesNotContain("SecretKey", body);
        Assert.DoesNotContain("Token", body);
        Assert.DoesNotContain("exception", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stacktrace", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetHealth_ShouldIncludeNoCacheHeaders_ToAvoidStaleHealthResponses()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.True(response.Headers.CacheControl?.NoCache == true ||
                    response.Headers.CacheControl?.NoStore == true);
    }

    [Fact]
    public async Task GetHealth_WithBearerToken_ShouldStillReturn200Healthy()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "arbitrary_or_expired_token_simulation");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body.Trim());
    }

    [Fact]
    public async Task GetHealth_ShouldBeIndependentFromBusinessControllersAndReports()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert: Responde texto plano nativo de Microsoft.AspNetCore.Diagnostics.HealthChecks
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body.Trim());
    }

    [Fact]
    public async Task GetHealth_ShouldBeIdempotentAndNotModifyData()
    {
        // Act: múltiples invocaciones consecutivas
        var response1 = await _client.GetAsync("/health");
        var response2 = await _client.GetAsync("/health");
        var response3 = await _client.GetAsync("/health");

        // Assert: no muta estado, siempre 200 y cuerpo idéntico
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var body1 = await response1.Content.ReadAsStringAsync();
        var body2 = await response2.Content.ReadAsStringAsync();
        var body3 = await response3.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", body1.Trim());
        Assert.Equal("Healthy", body2.Trim());
        Assert.Equal("Healthy", body3.Trim());
    }
}
