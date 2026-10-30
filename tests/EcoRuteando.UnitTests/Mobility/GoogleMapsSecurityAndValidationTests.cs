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
    [InlineData("Geocode")]
    [InlineData("ReverseGeocode")]
    public void ProtectedEndpoints_ShouldRequireAuthorize(string methodName)
    {
        var method = typeof(GoogleMapsController).GetMethod(methodName);
        Assert.NotNull(method);

        var authAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
    }

    [Theory]
    [InlineData("GetDirections")]
    [InlineData("GetPlacesNearby")]
    public void PublicEndpoints_ShouldAllowAnonymous(string methodName)
    {
        var method = typeof(GoogleMapsController).GetMethod(methodName);
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
    [InlineData("bicycling")]
    [InlineData("transit")]
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

    [Fact]
    public async Task GetDirections_WalkingRouteInNeiva_ReturnsOkWithPolyline()
    {
        // Parque Santander (2.9275, -75.2878) -> Malecón Río Magdalena (2.9312, -75.2934)
        var mockResponse = new DirectionsResponse
        {
            Status = "OK",
            EncodedPolyline = "}m_fE|g_eO~AgE|BqD",
            Distance = new DistanceInfo { Text = "1.1 km", ValueMeters = 1100 },
            Duration = new DurationInfo { Text = "14 mins", ValueSeconds = 840 },
            Steps = new List<RouteStep>
            {
                new() { HtmlInstructions = "Camina al oeste por Cl. 8 hacia Cra. 4", Distance = new DistanceInfo { Text = "300 m", ValueMeters = 300 } }
            }
        };

        _mapsServiceMock
            .Setup(s => s.GetDirectionsAsync(2.9275, -75.2878, 2.9312, -75.2934, "walking", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var result = await _controller.GetDirections(2.9275, -75.2878, 2.9312, -75.2934, "walking", CancellationToken.None);
        var okRes = Assert.IsType<OkObjectResult>(result);
        var val = Assert.IsType<DirectionsResponse>(okRes.Value);

        Assert.Equal("OK", val.Status);
        Assert.Equal("}m_fE|g_eO~AgE|BqD", val.EncodedPolyline);
        Assert.Equal(1100, val.Distance.ValueMeters);
        Assert.Equal(840, val.Duration.ValueSeconds);
    }

    [Fact]
    public async Task GetDirections_DrivingRouteInNeiva_ReturnsOkWithPolyline()
    {
        // Terminal Neiva (2.9150, -75.2750) -> USCO (2.9420, -75.2980)
        var mockResponse = new DirectionsResponse
        {
            Status = "OK",
            EncodedPolyline = "_w~eExj~dOaDbCw@f@",
            Distance = new DistanceInfo { Text = "4.8 km", ValueMeters = 4800 },
            Duration = new DurationInfo { Text = "11 mins", ValueSeconds = 660 }
        };

        _mapsServiceMock
            .Setup(s => s.GetDirectionsAsync(2.9150, -75.2750, 2.9420, -75.2980, "driving", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var result = await _controller.GetDirections(2.9150, -75.2750, 2.9420, -75.2980, "driving", CancellationToken.None);
        var okRes = Assert.IsType<OkObjectResult>(result);
        var val = Assert.IsType<DirectionsResponse>(okRes.Value);

        Assert.Equal("OK", val.Status);
        Assert.Equal(4800, val.Distance.ValueMeters);
    }

    [Theory]
    [InlineData("driving", "caminando")]
    [InlineData("walking", "autom")]
    public async Task GetDirections_ZeroResults_ReturnsNotFoundWithFriendlySuggestion(string travelMode, string expectedSuggestion)
    {
        _mapsServiceMock
            .Setup(s => s.GetDirectionsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), travelMode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DirectionsResponse { Status = "ZERO_RESULTS" });

        var result = await _controller.GetDirections(2.92, -75.28, 2.94, -75.29, travelMode, CancellationToken.None);
        var notFoundRes = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundRes.Value);

        var json = System.Text.Json.JsonSerializer.Serialize(
            notFoundRes.Value,
            new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        Assert.Contains(expectedSuggestion, json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetDirections_ProviderFailure_Returns502BadGateway()
    {
        _mapsServiceMock
            .Setup(s => s.GetDirectionsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), "driving", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DirectionsResponse?)null);

        var result = await _controller.GetDirections(2.92, -75.28, 2.94, -75.29, "driving", CancellationToken.None);
        var statusRes = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, statusRes.StatusCode);
    }

    [Fact]
    public void GetDirections_HasAllowAnonymous_AndDoesNotHaveAuthorize()
    {
        var method = typeof(GoogleMapsController).GetMethod("GetDirections");
        Assert.NotNull(method);

        var allowAnon = method.GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.NotNull(allowAnon);

        var authAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.Null(authAttr);
    }
}
