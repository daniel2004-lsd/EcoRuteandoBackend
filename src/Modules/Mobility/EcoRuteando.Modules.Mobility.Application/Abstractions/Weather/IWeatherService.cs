namespace EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;

public interface IWeatherService
{
    Task<WeatherConditionsResponse?> GetCurrentConditionsAsync(
        double lat,
        double lng,
        CancellationToken cancellationToken = default);

    Task<List<WeatherAlertResponse>?> GetWeatherAlertsAsync(
        double lat,
        double lng,
        string? languageCode = null,
        CancellationToken cancellationToken = default);
}

public sealed class WeatherConditionsResponse
{
    public string Condition { get; init; } = string.Empty;
    public string? ConditionDescription { get; init; }
    public string IconBaseUri { get; init; } = string.Empty;
    public double TemperatureC { get; init; }
    public double FeelsLikeC { get; init; }
    public int RelativeHumidity { get; init; }
    public double WindKmh { get; init; }
    public double GustKmh { get; init; }
    public int PrecipitationProbability { get; init; }
    public int ThunderstormProbability { get; init; }
    public int CloudCover { get; init; }
    public int UvIndex { get; init; }
    public bool IsDaytime { get; init; }
    public string TimeZone { get; init; } = string.Empty;
}

public sealed class WeatherAlertResponse
{
    public string AlertTitle { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Instruction { get; init; }
    public string? AreaName { get; init; }
    public DateTimeOffset? StartTime { get; init; }
    public DateTimeOffset? ExpirationTime { get; init; }
    public string? DataSource { get; init; }
}