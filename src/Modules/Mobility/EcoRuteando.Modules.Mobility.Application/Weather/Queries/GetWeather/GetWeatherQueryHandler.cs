using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;

public sealed class GetWeatherQueryHandler
    : IRequestHandler<GetWeatherQuery, GetWeatherResponse>
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<GetWeatherQueryHandler> _logger;

    public GetWeatherQueryHandler(
        IWeatherService weatherService,
        ILogger<GetWeatherQueryHandler> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    public async Task<GetWeatherResponse> Handle(
        GetWeatherQuery request,
        CancellationToken cancellationToken)
    {
        var originTask = _weatherService.GetCurrentConditionsAsync(
            request.OriginLat,
            request.OriginLng,
            cancellationToken);

        var destinationTask = _weatherService.GetCurrentConditionsAsync(
            request.DestinationLat,
            request.DestinationLng,
            cancellationToken);

        var originAlertsTask = _weatherService.GetWeatherAlertsAsync(
            request.OriginLat,
            request.OriginLng,
            languageCode: "es",
            cancellationToken);

        var destinationAlertsTask = _weatherService.GetWeatherAlertsAsync(
            request.DestinationLat,
            request.DestinationLng,
            languageCode: "es",
            cancellationToken);

        try
        {
            await Task.WhenAll(originTask, destinationTask, originAlertsTask, destinationAlertsTask);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Weather API failed; continuing without weather alerts");
            return new GetWeatherResponse
            {
                Origin = null,
                Destination = null,
                Alerts = [],
                Suggestions = []
            };
        }

        var origin = originTask.Result;
        var destination = destinationTask.Result;

        var alerts = new List<WeatherAlertResponse>();
        AddUniqueAlerts(alerts, originAlertsTask.Result);
        AddUniqueAlerts(alerts, destinationAlertsTask.Result);
        WeatherAlertsEvaluator.AddConditionsBasedAlerts(alerts, origin);
        WeatherAlertsEvaluator.AddConditionsBasedAlerts(alerts, destination);

        var suggestions = WeatherAlertsEvaluator.BuildSuggestions(alerts);

        return new GetWeatherResponse
        {
            Origin = origin,
            Destination = destination,
            Alerts = alerts,
            Suggestions = suggestions
        };
    }

    private static void AddUniqueAlerts(
        List<WeatherAlertResponse> target,
        IReadOnlyCollection<WeatherAlertResponse>? source)
    {
        if (source is null)
        {
            return;
        }

        foreach (var alert in source)
        {
            if (!target.Any(a =>
                    a.EventType == alert.EventType &&
                    a.AlertTitle == alert.AlertTitle &&
                    a.StartTime == alert.StartTime))
            {
                target.Add(alert);
            }
        }
    }
}