using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;
using EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeatherCurrent;

public sealed record GetWeatherCurrentResponse
{
    public WeatherPointResponse? Weather { get; init; }
    public List<WeatherAlertResponse> Alerts { get; init; } = [];
    public List<WeatherForecastDayResponse>? Forecast { get; init; }
    public List<string> Suggestions { get; init; } = [];
}