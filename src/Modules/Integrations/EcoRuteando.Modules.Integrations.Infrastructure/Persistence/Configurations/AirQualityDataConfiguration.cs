using EcoRuteando.Modules.Integrations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Configurations;

public class AirQualityDataConfiguration : IEntityTypeConfiguration<AirQualityData>
{
    public void Configure(EntityTypeBuilder<AirQualityData> builder)
    {
        builder.ToTable("air_quality_data", "integrations");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.Location)
            .HasColumnName("location")
            .HasColumnType("geography(Point, 4326)")
            .IsRequired();

        builder.Property(a => a.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(a => a.Pm25)
            .HasColumnName("pm25")
            .HasPrecision(8, 2);

        builder.Property(a => a.Pm10)
            .HasColumnName("pm10")
            .HasPrecision(8, 2);

        builder.Property(a => a.Co2Ppm)
            .HasColumnName("co2_ppm")
            .HasPrecision(8, 2);

        builder.Property(a => a.Aqi)
            .HasColumnName("aqi");

        builder.Property(a => a.Source)
            .HasColumnName("source")
            .HasMaxLength(100);

        builder.Property(a => a.RawData)
            .HasColumnName("raw_data")
            .HasColumnType("jsonb");

        builder.Property(a => a.MeasuredAt)
            .HasColumnName("measured_at")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
