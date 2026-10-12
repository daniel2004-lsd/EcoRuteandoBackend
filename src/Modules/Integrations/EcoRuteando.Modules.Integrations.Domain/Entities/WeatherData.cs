using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Integrations.Domain.Entities;

public class WeatherData
{
    public Guid Id { get; private set; }
    public Point Location { get; private set; } = default!;
    public string? City { get; private set; }
    public decimal? TemperatureC { get; private set; }
    public short? HumidityPct { get; private set; }
    public decimal? WindSpeedKmh { get; private set; }
    public string? Condition { get; private set; }
    public short? RainProbability { get; private set; }
    public string? RawData { get; private set; }
    public string? Source { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private WeatherData() { }

    public static WeatherData Create(
        double latitude,
        double longitude,
        string? city,
        decimal? temperatureC,
        short? humidityPct,
        decimal? windSpeedKmh,
        string? condition,
        short? rainProbability,
        string? rawData,
        string? source,
        DateTime validFrom,
        DateTime validUntil)
    {
        if (validUntil <= validFrom)
            throw new ArgumentException("ValidUntil must be greater than ValidFrom.");

        var point = new Point(longitude, latitude) { SRID = 4326 };

        return new WeatherData
        {
            Id = Guid.NewGuid(),
            Location = point,
            City = string.IsNullOrWhiteSpace(city) ? null : city.Trim(),
            TemperatureC = temperatureC,
            HumidityPct = humidityPct.HasValue ? (short)Math.Clamp((int)humidityPct.Value, 0, 100) : null,
            WindSpeedKmh = windSpeedKmh,
            Condition = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim(),
            RainProbability = rainProbability.HasValue ? (short)Math.Clamp((int)rainProbability.Value, 0, 100) : null,
            RawData = rawData,
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            CreatedAt = DateTime.UtcNow
        };
    }
}
