namespace EcoRuteando.Modules.Integrations.Application.Environmental.Dtos;

public record AirQualityDto(
    Guid Id,
    double Latitude,
    double Longitude,
    string? City,
    int? Aqi,
    string AqiCategory,
    decimal? Pm25,
    decimal? Pm10,
    decimal? Co2Ppm,
    string? Source,
    DateTime MeasuredAt);
