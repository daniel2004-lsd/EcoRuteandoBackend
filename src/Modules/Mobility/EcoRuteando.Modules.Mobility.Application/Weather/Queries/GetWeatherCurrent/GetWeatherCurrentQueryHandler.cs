using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;
using EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeatherCurrent;

public sealed class GetWeatherCurrentQueryHandler
    : IRequestHandler<GetWeatherCurrentQuery, GetWeatherCurrentResponse>
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<GetWeatherCurrentQueryHandler> _logger;

    public GetWeatherCurrentQueryHandler(
        IWeatherService weatherService,
        ILogger<GetWeatherCurrentQueryHandler> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    public async Task<GetWeatherCurrentResponse> Handle(
        GetWeatherCurrentQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var weatherTask = _weatherService.GetCurrentConditionsAsync(
                request.Lat,
                request.Lng,
                cancellationToken);

            var alertsTask = _weatherService.GetWeatherAlertsAsync(
                request.Lat,
                request.Lng,
                languageCode: "es",
                cancellationToken);

            var forecastTask = _weatherService.GetDailyForecastAsync(
                request.Lat,
                request.Lng,
                days: 5,
                languageCode: "es",
                cancellationToken);

            await Task.WhenAll(weatherTask, alertsTask, forecastTask);

            var alerts = alertsTask.Result ?? [];
            WeatherAlertsEvaluator.AddConditionsBasedAlerts(alerts, weatherTask.Result);

            return new GetWeatherCurrentResponse
            {
                Weather = weatherTask.Result,
                Alerts = alerts,
                Forecast = forecastTask.Result,
                Suggestions = WeatherAlertsEvaluator.BuildSuggestions(alerts)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Weather API failed; returning empty weather");
            return new GetWeatherCurrentResponse();
        }
    }
}