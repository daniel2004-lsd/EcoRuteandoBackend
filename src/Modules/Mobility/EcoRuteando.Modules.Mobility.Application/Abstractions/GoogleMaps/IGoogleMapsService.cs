namespace EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;

public interface IGoogleMapsService
{
    Task<DirectionsResponse?> GetDirectionsAsync(
        double originLat,
        double originLng,
        double destinationLat,
        double destinationLng,
        string travelMode,
        CancellationToken cancellationToken = default);

    Task<GeocodingResponse?> GeocodeAddressAsync(
        string address,
        CancellationToken cancellationToken = default);

    Task<ReverseGeocodingResponse?> ReverseGeocodeAsync(
        double lat,
        double lng,
        CancellationToken cancellationToken = default);
}

public sealed class DirectionsResponse
{
    public string Status { get; init; } = string.Empty;
    public string? EncodedPolyline { get; init; }
    public DistanceInfo? Distance { get; init; }
    public DurationInfo? Duration { get; init; }
    public List<RouteStep>? Steps { get; init; }
    public ViewportBounds? Bounds { get; init; }
}

public sealed class DistanceInfo
{
    public string Text { get; init; } = string.Empty;
    public int ValueMeters { get; init; }
}

public sealed class DurationInfo
{
    public string Text { get; init; } = string.Empty;
    public int ValueSeconds { get; init; }
}

public sealed class RouteStep
{
    public string HtmlInstructions { get; init; } = string.Empty;
    public DistanceInfo? Distance { get; init; }
    public DurationInfo? Duration { get; init; }
    public string? TravelMode { get; init; }
    public StepLocation? StartLocation { get; init; }
    public StepLocation? EndLocation { get; init; }
}

public sealed class StepLocation
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}

public sealed class ViewportBounds
{
    public GeoPoint? Northeast { get; init; }
    public GeoPoint? Southwest { get; init; }
}

public sealed class GeoPoint
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}

public sealed class GeocodingResponse
{
    public string Status { get; init; } = string.Empty;
    public List<GeocodingResult>? Results { get; init; }
}

public sealed class GeocodingResult
{
    public string FormattedAddress { get; init; } = string.Empty;
    public double Lat { get; init; }
    public double Lng { get; init; }
    public string? PlaceId { get; init; }
    public List<AddressComponent>? AddressComponents { get; init; }
}

public sealed class AddressComponent
{
    public string LongName { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public List<string>? Types { get; init; }
}

public sealed class ReverseGeocodingResponse
{
    public string Status { get; init; } = string.Empty;
    public List<GeocodingResult>? Results { get; init; }
}