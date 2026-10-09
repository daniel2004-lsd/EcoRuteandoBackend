namespace EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;

public sealed class GoogleWeatherOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://weather.googleapis.com/v1";
}