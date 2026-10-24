using EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;
using EcoRuteando.Modules.Mobility.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moq;
using System.Reflection;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility;

public class GoogleMapsSecurityAndValidationTests
{
    private readonly Mock<IGoogleMapsService> _mapsServiceMock;
    private readonly GoogleMapsController _controller;

    public GoogleMapsSecurityAndValidationTests()
    {
        _mapsServiceMock = new Mock<IGoogleMapsService>();
        _controller = new GoogleMapsController(_mapsServiceMock.Object);
    }

    [Fact]
    public void Controller_ShouldHaveRateLimitingAttribute()
    {
        var attr = typeof(GoogleMapsController).GetCustomAttribute<EnableRateLimitingAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("api", attr.PolicyName);
    }

    [Theory]
    [InlineData("GetDirections")]
    [InlineData("Geocode")]
    [InlineData("ReverseGeocode")]
    public void ProtectedEndpoints_ShouldRequireAuthorize(string methodName)
    {
        var method = typeof(GoogleMapsController).GetMethod(methodName);
        Assert.NotNull(method);

        var authAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
    }

    [Fact]
    public void PlacesEndpoint_ShouldAllowAnonymous()
    {
        var method = typeof(GoogleMapsController).GetMethod("GetPlacesNearby");
        Assert.NotNull(method);

        var allowAnon = method.GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.NotNull(allowAnon);
    }

    [Theory]
    [InlineData(-91.0, 0, 0, 0)]
    [InlineData(91.0, 0, 0, 0)]
    [InlineData(0, -181.0, 0, 0)]
    [InlineData(0, 181.0, 0, 0)]
    [InlineData(0, 0, -95.0, 0)]
    [InlineData(0, 0, 0, 200.0)]
    public async Task GetDirections_InvalidCoordinates_ReturnsBadRequest(double oLat, double oLng, double dLat, double dLng)
    {
        var result = await _controller.GetDirections(oLat, oLng, dLat, dLng, "driving", CancellationToken.None);
        var badReq = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badReq.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("plane")]
    [InlineData("submarine")]
    public async Task GetDirections_InvalidTravelMode_ReturnsBadRequest(string travelMode)
    {
        var result = await _controller.GetDirections(2.92, -75.28, 2.94, -75.29, travelMode, CancellationToken.None);
        var badReq = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badReq.Value);
    }

    [Fact]
    public async Task Geocode_EmptyOrExcessiveAddress_ReturnsBadRequest()
    {
        var emptyRes = await _controller.Geocode("", CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(emptyRes);

        var longAddress = new string('A', 201);
        var longRes = await _controller.Geocode(longAddress, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(longRes);
    }

    [Theory]
    [InlineData(-100, 0)]
    [InlineData(100, 0)]
    [InlineData(0, -190)]
    [InlineData(0, 190)]
    public async Task ReverseGeocode_InvalidCoordinates_ReturnsBadRequest(double lat, double lng)
    {
        var result = await _controller.ReverseGeocode(lat, lng, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDirections_ValidRequest_DoesNotLeakSecretsInResponse()
    {
        var mockResponse = new DirectionsResponse
        {
            Status = "OK",
            EncodedPolyline = "abcdef_polyline",
            Distance = new DistanceInfo { Text = "5.2 km", ValueMeters = 5200 },
            Duration = new DurationInfo { Text = "12 mins", ValueSeconds = 720 }
        };

        _mapsServiceMock
            .Setup(s => s.GetDirectionsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var result = await _controller.GetDirections(2.92, -75.28, 2.94, -75.29, "driving", CancellationToken.None);
        var okRes = Assert.IsType<OkObjectResult>(result);
        var val = Assert.IsType<DirectionsResponse>(okRes.Value);

        Assert.Equal("OK", val.Status);
        Assert.Equal("abcdef_polyline", val.EncodedPolyline);

        // Ensure no API keys or internal urls are present
        var json = System.Text.Json.JsonSerializer.Serialize(val);
        Assert.DoesNotContain("key=", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ApiKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AIzaSy", json);
    }
}
