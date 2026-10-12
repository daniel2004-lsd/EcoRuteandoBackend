using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestWeather;

public record IngestWeatherCommand(
    double Latitude,
    double Longitude,
    string? City,
    decimal? TemperatureC,
    short? HumidityPct,
    decimal? WindSpeedKmh,
    string? Condition,
    short? RainProbability,
    string? RawData,
    string? Source,
    DateTime ValidFrom,
    DateTime ValidUntil) : IRequest<WeatherDto>;
