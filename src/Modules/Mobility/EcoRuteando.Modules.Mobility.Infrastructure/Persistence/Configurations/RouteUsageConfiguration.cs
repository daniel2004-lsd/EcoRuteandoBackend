using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class RouteUsageConfiguration
    : IEntityTypeConfiguration<RouteUsage>
{
    public void Configure(EntityTypeBuilder<RouteUsage> builder)
    {
        builder.ToTable("route_usage", "mobility");

        // PK compuesta (usage_id, created_at) igual que el DDL particionado
        builder.HasKey(ru => new { ru.Id, ru.CreatedAt });

        builder.Property(ru => ru.Id)
            .HasColumnName("usage_id");

        builder.Property(ru => ru.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ru => ru.RouteId)
            .HasColumnName("route_id")
            .IsRequired();

        builder.Property(ru => ru.TransportMode)
            .HasColumnName("transport_mode")
            .HasColumnType("transport_type");

        builder.Property(ru => ru.StartedAt)
            .HasColumnName("started_at")
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ru => ru.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(ru => ru.Completed)
            .HasColumnName("completed")
            .HasDefaultValue(false);

        builder.Property(ru => ru.ActualDistanceKm)
            .HasColumnName("actual_distance_km")
            .HasColumnType("numeric(10,2)");

        builder.Property(ru => ru.ActualDurationMin)
            .HasColumnName("actual_duration_min");

        builder.Property(ru => ru.ActualCo2Kg)
            .HasColumnName("actual_co2_kg")
            .HasColumnType("numeric(10,4)");

        // Columna PostGIS GEOGRAPHY(LINESTRING, 4326)
        builder.Property(ru => ru.ActualRoute)
            .HasColumnName("actual_route");

        builder.Property(ru => ru.GpsData)
            .HasColumnName("gps_data")
            .HasColumnType("jsonb");

        builder.Property(ru => ru.Source)
            .HasColumnName("source")
            .HasColumnType("usage_source")
            .IsRequired();

        builder.Property(ru => ru.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(ru => ru.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "chk_route_usage_actual_distance_km",
                "actual_distance_km IS NULL OR actual_distance_km >= 0");

            t.HasCheckConstraint(
                "chk_route_usage_actual_duration_min",
                "actual_duration_min IS NULL OR actual_duration_min >= 0");

            t.HasCheckConstraint(
                "chk_route_usage_gps_data",
                "gps_data IS NULL OR jsonb_typeof(gps_data) = 'array'");

            t.HasCheckConstraint(
                "chk_route_usage_ended_after_started",
                "ended_at IS NULL OR ended_at >= started_at");
        });

        builder.HasOne(ru => ru.Route)
            .WithMany()
            .HasForeignKey(ru => ru.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
