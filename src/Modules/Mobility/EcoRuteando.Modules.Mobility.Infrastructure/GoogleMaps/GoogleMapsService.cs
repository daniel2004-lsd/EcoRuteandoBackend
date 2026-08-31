using System.Net.Http.Json;
using System.Text.Json;
using EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Mobility.Infrastructure.GoogleMaps;

public sealed class GoogleMapsService : IGoogleMapsService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleMapsOptions _options;
    private readonly ILogger<GoogleMapsService> _logger;

    public GoogleMapsService(
        HttpClient httpClient,
        IOptions<GoogleMapsOptions> options,
        ILogger<GoogleMapsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DirectionsResponse?> GetDirectionsAsync(
        double originLat,
        double originLng,
        double destinationLat,
        double destinationLng,
        string travelMode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var origin = $"{originLat},{originLng}";
            var destination = $"{destinationLat},{destinationLng}";

            var url = $"{_options.DirectionsBaseUrl}" +
                      $"?origin={origin}" +
                      $"&destination={destination}" +
                      $"&mode={travelMode}" +
                      $"&key={_options.ApiKey}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Directions API returned status {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = JsonDocument.Parse(json);

            var root = document.RootElement;
            var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

            if (status != "OK")
            {
                _logger.LogWarning(
                    "Google Directions API returned status {Status}",
                    status);

                if (status == "ZERO_RESULTS")
                {
                    return new DirectionsResponse { Status = status };
                }

                return null;
            }

            var routes = root.GetProperty("routes");
            if (routes.GetArrayLength() == 0)
            {
                return null;
            }

            var route = routes[0];
            var leg = route.GetProperty("legs")[0];

            var encodedPolyline = route
                .GetProperty("overview_polyline")
                .GetProperty("points")
                .GetString();

            var distance = leg.GetProperty("distance");
            var duration = leg.GetProperty("duration");

            var steps = new List<RouteStep>();
            foreach (var step in leg.GetProperty("steps").EnumerateArray())
            {
                steps.Add(new RouteStep
                {
                    HtmlInstructions = step.GetProperty("html_instructions").GetString() ?? string.Empty,
                    Distance = ParseDistance(step.GetProperty("distance")),
                    Duration = ParseDuration(step.GetProperty("duration")),
                    TravelMode = step.GetProperty("travel_mode").GetString(),
                    StartLocation = ParseLocation(step.GetProperty("start_location")),
                    EndLocation = ParseLocation(step.GetProperty("end_location"))
                });
            }

            var bounds = ParseBounds(route.GetProperty("bounds"));

            return new DirectionsResponse
            {
                Status = status,
                EncodedPolyline = encodedPolyline,
                Distance = ParseDistance(distance),
                Duration = ParseDuration(duration),
                Steps = steps,
                Bounds = bounds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Directions API");
            return null;
        }
    }

    public async Task<GeocodingResponse?> GeocodeAddressAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_options.GeocodeBaseUrl}" +
                      $"?address={Uri.EscapeDataString(address)}" +
                      $"&key={_options.ApiKey}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Geocoding API returned status {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = JsonDocument.Parse(json);

            var root = document.RootElement;
            var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

            if (status != "OK")
            {
                _logger.LogWarning(
                    "Google Geocoding API returned status {Status}",
                    status);
                return null;
            }

            var results = new List<GeocodingResult>();
            foreach (var result in root.GetProperty("results").EnumerateArray())
            {
                var location = result
                    .GetProperty("geometry")
                    .GetProperty("location");

                results.Add(new GeocodingResult
                {
                    FormattedAddress = result.GetProperty("formatted_address").GetString() ?? string.Empty,
                    Lat = location.GetProperty("lat").GetDouble(),
                    Lng = location.GetProperty("lng").GetDouble(),
                    PlaceId = result.TryGetProperty("place_id", out var placeId)
                        ? placeId.GetString()
                        : null,
                    AddressComponents = ParseAddressComponents(result)
                });
            }

            return new GeocodingResponse
            {
                Status = status,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Geocoding API");
            return null;
        }
    }

    public async Task<ReverseGeocodingResponse?> ReverseGeocodeAsync(
        double lat,
        double lng,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_options.GeocodeBaseUrl}" +
                      $"?latlng={lat},{lng}" +
                      $"&key={_options.ApiKey}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Google Reverse Geocoding API returned status {StatusCode}",
                    response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = JsonDocument.Parse(json);

            var root = document.RootElement;
            var status = root.GetProperty("status").GetString() ?? "UNKNOWN";

            if (status != "OK")
            {
                return null;
            }

            var results = new List<GeocodingResult>();
            foreach (var result in root.GetProperty("results").EnumerateArray())
            {
                var location = result
                    .GetProperty("geometry")
                    .GetProperty("location");

                results.Add(new GeocodingResult
                {
                    FormattedAddress = result.GetProperty("formatted_address").GetString() ?? string.Empty,
                    Lat = location.GetProperty("lat").GetDouble(),
                    Lng = location.GetProperty("lng").GetDouble(),
                    PlaceId = result.TryGetProperty("place_id", out var placeId)
                        ? placeId.GetString()
                        : null,
                    AddressComponents = ParseAddressComponents(result)
                });
            }

            return new ReverseGeocodingResponse
            {
                Status = status,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Reverse Geocoding API");
            return null;
        }
    }

    private static DistanceInfo ParseDistance(JsonElement element)
    {
        return new DistanceInfo
        {
            Text = element.GetProperty("text").GetString() ?? string.Empty,
            ValueMeters = element.GetProperty("value").GetInt32()
        };
    }

    private static DurationInfo ParseDuration(JsonElement element)
    {
        return new DurationInfo
        {
            Text = element.GetProperty("text").GetString() ?? string.Empty,
            ValueSeconds = element.GetProperty("value").GetInt32()
        };
    }

    private static StepLocation ParseLocation(JsonElement element)
    {
        return new StepLocation
        {
            Lat = element.GetProperty("lat").GetDouble(),
            Lng = element.GetProperty("lng").GetDouble()
        };
    }

    private static ViewportBounds ParseBounds(JsonElement element)
    {
        return new ViewportBounds
        {
            Northeast = new GeoPoint
            {
                Lat = element.GetProperty("northeast").GetProperty("lat").GetDouble(),
                Lng = element.GetProperty("northeast").GetProperty("lng").GetDouble()
            },
            Southwest = new GeoPoint
            {
                Lat = element.GetProperty("southwest").GetProperty("lat").GetDouble(),
                Lng = element.GetProperty("southwest").GetProperty("lng").GetDouble()
            }
        };
    }

    private static List<AddressComponent>? ParseAddressComponents(JsonElement result)
    {
        if (!result.TryGetProperty("address_components", out var components))
        {
            return null;
        }

        var list = new List<AddressComponent>();
        foreach (var component in components.EnumerateArray())
        {
            var types = new List<string>();
            foreach (var type in component.GetProperty("types").EnumerateArray())
            {
                types.Add(type.GetString() ?? string.Empty);
            }

            list.Add(new AddressComponent
            {
                LongName = component.GetProperty("long_name").GetString() ?? string.Empty,
                ShortName = component.GetProperty("short_name").GetString() ?? string.Empty,
                Types = types
            });
        }

        return list;
    }
}