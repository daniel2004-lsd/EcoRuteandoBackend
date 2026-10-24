using EcoRuteando.Modules.Integrations.Domain.Entities;
using Xunit;

namespace EcoRuteando.UnitTests.Integrations.Entities;

public sealed class ApiCacheEntryTests
{
    [Theory]
    [InlineData("", "endpoint", "response")]
    [InlineData("   ", "endpoint", "response")]
    [InlineData("provider", "", "response")]
    [InlineData("provider", "   ", "response")]
    [InlineData("provider", "endpoint", "")]
    [InlineData("provider", "endpoint", "   ")]
    public void Create_WithEmptyRequiredFields_ThrowsArgumentException(string provider, string endpoint, string response)
    {
        Assert.Throws<ArgumentException>(() =>
            ApiCacheEntry.Create(provider, endpoint, "{}", response, 300));
    }

    [Fact]
    public void Create_WithValidParameters_ComputesHashAndSetsExpiration()
    {
        var entry = ApiCacheEntry.Create(
            provider: "  OpenRouteService  ",
            endpoint: "  /v2/directions/cycling-regular  ",
            parameters: "{\"start\": [4.6, -74.0]}",
            response: "{\"routes\": []}",
            ttlSeconds: 600);

        Assert.Equal("OpenRouteService", entry.Provider);
        Assert.Equal("/v2/directions/cycling-regular", entry.Endpoint);
        Assert.Equal("{\"start\": [4.6, -74.0]}", entry.Parameters);
        Assert.False(string.IsNullOrWhiteSpace(entry.ParametersHash));
        Assert.True(entry.IsValid);
        Assert.True(entry.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void ComputeParametersHash_ReturnsDeterministicSha256Hex()
    {
        var hash1 = ApiCacheEntry.ComputeParametersHash("{\"query\":\"ciclovia\"}");
        var hash2 = ApiCacheEntry.ComputeParametersHash("{\"query\":\"ciclovia\"}");
        var hash3 = ApiCacheEntry.ComputeParametersHash("{\"query\":\"parque\"}");

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.Equal(64, hash1.Length); // 256 bits = 64 hex chars
    }

    [Fact]
    public void Invalidate_SetsIsValidToFalse()
    {
        var entry = ApiCacheEntry.Create("Nominatim", "/search", "{}", "{}", 3600);
        Assert.True(entry.IsValid);

        entry.Invalidate();

        Assert.False(entry.IsValid);
    }
}
