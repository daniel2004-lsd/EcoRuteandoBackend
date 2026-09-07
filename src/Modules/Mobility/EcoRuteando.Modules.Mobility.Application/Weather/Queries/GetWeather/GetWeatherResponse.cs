using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;

public sealed record GetWeatherResponse
{
    public WeatherPointResponse? Origin { get; init; }
    public WeatherPointResponse? Destination { get; init; }
    public List<WeatherAlertResponse> Alerts { get; init; } = [];
    public List<string> Suggestions { get; init; } = [];
}

public sealed record WeatherPointResponse
{
    public string? Condition { get; init; }
    public string? ConditionDescription { get; init; }
    public string? IconBaseUri { get; init; }
    public double? TemperatureC { get; init; }
    public double? FeelsLikeC { get; init; }
    public int? RelativeHumidity { get; init; }
    public double? WindKmh { get; init; }
    public int? PrecipitationProbability { get; init; }
    public int? ThunderstormProbability { get; init; }
    public bool? IsDaytime { get; init; }
    public string? TimeZone { get; init; }

    public static implicit operator WeatherPointResponse(WeatherConditionsResponse? conditions)
        => conditions is null
            ? null
            : new WeatherPointResponse
            {
                Condition = conditions.Condition,
                ConditionDescription = conditions.ConditionDescription,
                IconBaseUri = conditions.IconBaseUri,
                TemperatureC = conditions.TemperatureC,
                FeelsLikeC = conditions.FeelsLikeC,
                RelativeHumidity = conditions.RelativeHumidity,
                WindKmh = conditions.WindKmh,
                PrecipitationProbability = conditions.PrecipitationProbability,
                ThunderstormProbability = conditions.ThunderstormProbability,
                IsDaytime = conditions.IsDaytime,
                TimeZone = conditions.TimeZone
            };
}