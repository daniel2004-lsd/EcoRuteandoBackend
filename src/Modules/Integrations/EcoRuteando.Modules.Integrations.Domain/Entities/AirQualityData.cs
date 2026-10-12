using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Integrations.Domain.Entities;

public class AirQualityData
{
    public Guid Id { get; private set; }
    public Point Location { get; private set; } = default!;
    public string? City { get; private set; }
    public decimal? Pm25 { get; private set; }
    public decimal? Pm10 { get; private set; }
    public decimal? Co2Ppm { get; private set; }
    public int? Aqi { get; private set; }
    public string? Source { get; private set; }
    public string? RawData { get; private set; }
    public DateTime MeasuredAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AirQualityData() { }

    public static AirQualityData Create(
        double latitude,
        double longitude,
        string? city,
        int? aqi,
        decimal? pm25,
        decimal? pm10,
        decimal? co2Ppm,
        string? source,
        string? rawData,
        DateTime? measuredAt = null)
    {
        var point = new Point(longitude, latitude) { SRID = 4326 };

        return new AirQualityData
        {
            Id = Guid.NewGuid(),
            Location = point,
            City = string.IsNullOrWhiteSpace(city) ? null : city.Trim(),
            Aqi = aqi,
            Pm25 = pm25,
            Pm10 = pm10,
            Co2Ppm = co2Ppm,
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            RawData = rawData,
            MeasuredAt = measuredAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
