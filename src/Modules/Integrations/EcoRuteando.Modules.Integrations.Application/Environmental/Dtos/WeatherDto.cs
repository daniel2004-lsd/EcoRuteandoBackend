namespace EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;

public record WeatherDto(
    Guid Id,
    double Latitude,
    double Longitude,
    string? City,
    decimal? TemperatureC,
    short? HumidityPct,
    decimal? WindSpeedKmh,
    string? Condition,
    short? RainProbability,
    string? Source,
    DateTime ValidFrom,
    DateTime ValidUntil);
