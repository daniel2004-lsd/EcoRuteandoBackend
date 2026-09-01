namespace EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;

public sealed class GoogleMapsOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string DirectionsBaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/directions/json";

    public string GeocodeBaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/geocode/json";
}