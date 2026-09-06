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
        AddConditionsBasedAlerts(alerts, origin);
        AddConditionsBasedAlerts(alerts, destination);

        var suggestions = BuildSuggestions(alerts);

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

    private static void AddConditionsBasedAlerts(
        List<WeatherAlertResponse> target,
        WeatherConditionsResponse? conditions)
    {
        if (conditions is null)
        {
            return;
        }

        if (conditions.TemperatureC >= 35)
        {
            target.Add(new WeatherAlertResponse
            {
                EventType = "HEAT",
                AlertTitle = "Temperatura muy alta",
                Severity = "MODERATE",
                Description = $"Se esperan temperaturas cercanas a {conditions.TemperatureC:0}°C en el trayecto.",
                Instruction = "Mantente hidratado, evita la exposición prolongada y usa protección solar."
            });
        }

        if (conditions.ThunderstormProbability >= 50)
        {
            target.Add(new WeatherAlertResponse
            {
                EventType = "THUNDERSTORM",
                AlertTitle = "Probabilidad de tormenta",
                Severity = "MODERATE",
                Description = $"El trayecto tiene un {conditions.ThunderstormProbability}% de probabilidad de tormenta.",
                Instruction = "Considera retrasar la salida o usar un medio de transporte con techo."
            });
        }
        else if (conditions.PrecipitationProbability >= 50)
        {
            target.Add(new WeatherAlertResponse
            {
                EventType = "RAIN",
                AlertTitle = "Lluvia probable",
                Severity = "MINOR",
                Description = $"Hay un {conditions.PrecipitationProbability}% de probabilidad de lluvia en el trayecto.",
                Instruction = "Lleva protección para la lluvia o usa transporte público."
            });
        }

        if (conditions.WindKmh >= 40)
        {
            target.Add(new WeatherAlertResponse
            {
                EventType = "WIND",
                AlertTitle = "Vientos fuertes",
                Severity = "MINOR",
                Description = $"Se registran vientos de hasta {conditions.WindKmh:0} km/h en el trayecto.",
                Instruction = "Ten precaución si viajas en bicicleta o moto."
            });
        }
    }

    private static List<string> BuildSuggestions(IReadOnlyCollection<WeatherAlertResponse> alerts)
    {
        var suggestions = new List<string>();

        if (alerts.Any(a => a.EventType is "RAIN" or "THUNDERSTORM" or "HEAT"))
        {
            suggestions.Add(
                "Las condiciones climáticas podrían afectar tu trayecto. " +
                "Considera preferir transporte público o vehículo particular.");
        }

        if (alerts.Any(a => a.EventType == "WIND"))
        {
            suggestions.Add(
                "Con vientos fuertes, prefiere caminar o usar transporte público de ruta fija.");
        }

        return suggestions.Distinct().ToList();
    }
}