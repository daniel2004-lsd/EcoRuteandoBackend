using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries;

/// <summary>
/// Genera alertas y sugerencias a partir de condiciones climáticas cuando no
/// existen alertas oficiales emitidas por las autoridades (Google publicAlerts).
/// </summary>
public static class WeatherAlertsEvaluator
{
    public static List<WeatherAlertResponse> AddConditionsBasedAlerts(
        List<WeatherAlertResponse> target,
        WeatherConditionsResponse? conditions)
    {
        if (conditions is null)
        {
            return target;
        }

        if (conditions.TemperatureC >= 35)
        {
            target.Add(Alert(
                "HEAT",
                "Temperatura muy alta",
                "MODERATE",
                $"Se esperan temperaturas cercanas a {conditions.TemperatureC:0}°C en el trayecto.",
                "Mantente hidratado, evita la exposición prolongada y usa protección solar."));
        }

        if (conditions.ThunderstormProbability >= 50)
        {
            target.Add(Alert(
                "THUNDERSTORM",
                "Probabilidad de tormenta",
                "MODERATE",
                $"El trayecto tiene un {conditions.ThunderstormProbability}% de probabilidad de tormenta.",
                "Considera retrasar la salida o usar un medio de transporte con techo."));
        }
        else if (conditions.PrecipitationProbability >= 50)
        {
            target.Add(Alert(
                "RAIN",
                "Lluvia probable",
                "MINOR",
                $"Hay un {conditions.PrecipitationProbability}% de probabilidad de lluvia en el trayecto.",
                "Lleva protección para la lluvia o usa transporte público."));
        }

        if (conditions.WindKmh >= 40)
        {
            target.Add(Alert(
                "WIND",
                "Vientos fuertes",
                "MINOR",
                $"Se registran vientos de hasta {conditions.WindKmh:0} km/h en el trayecto.",
                "Ten precaución si viajas en bicicleta o moto."));
        }

        return target;
    }

    public static List<string> BuildSuggestions(IReadOnlyCollection<WeatherAlertResponse> alerts)
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

    private static WeatherAlertResponse Alert(
        string eventType,
        string title,
        string severity,
        string description,
        string instruction)
        => new()
        {
            EventType = eventType,
            AlertTitle = title,
            Severity = severity,
            Description = description,
            Instruction = instruction
        };
}