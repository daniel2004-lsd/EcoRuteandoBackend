using EcoRuteando.Modules.Integrations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Configurations;

public class WeatherDataConfiguration : IEntityTypeConfiguration<WeatherData>
{
    public void Configure(EntityTypeBuilder<WeatherData> builder)
    {
        builder.ToTable("weather_data", "integrations");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id");

        builder.Property(w => w.Location)
            .HasColumnName("location")
            .HasColumnType("geography(Point, 4326)")
            .IsRequired();

        builder.Property(w => w.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(w => w.TemperatureC)
            .HasColumnName("temperature_c")
            .HasPrecision(5, 2);

        builder.Property(w => w.HumidityPct)
            .HasColumnName("humidity_pct");

        builder.Property(w => w.WindSpeedKmh)
            .HasColumnName("wind_speed_kmh")
            .HasPrecision(6, 2);

        builder.Property(w => w.Condition)
            .HasColumnName("condition")
            .HasMaxLength(80);

        builder.Property(w => w.RainProbability)
            .HasColumnName("rain_probability");

        builder.Property(w => w.RawData)
            .HasColumnName("raw_data")
            .HasColumnType("jsonb");

        builder.Property(w => w.Source)
            .HasColumnName("source")
            .HasMaxLength(100);

        builder.Property(w => w.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(w => w.ValidUntil)
            .HasColumnName("valid_until")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
