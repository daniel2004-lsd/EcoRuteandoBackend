using EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestAirQuality;

public record IngestAirQualityCommand(
    double Latitude,
    double Longitude,
    string? City,
    int? Aqi,
    decimal? Pm25,
    decimal? Pm10,
    decimal? Co2Ppm,
    string? Source,
    string? RawData,
    DateTime? MeasuredAt = null) : IRequest<AirQualityDto>;
