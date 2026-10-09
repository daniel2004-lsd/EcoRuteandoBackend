using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes", "mobility");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Description)
            .HasColumnName("description");

        builder.Property(r => r.TransportType)
            .HasColumnName("transport_type")
            .HasColumnType("transport_type")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasColumnType("route_status")
            .IsRequired();

        builder.Property(r => r.StartName)
            .HasColumnName("start_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.DestinationName)
            .HasColumnName("destination_name")
            .IsRequired()
            .HasMaxLength(200);

        // Columnas PostGIS GEOGRAPHY (SRID 4326)
        builder.Property(r => r.StartLocation)
            .HasColumnName("start_location");

        builder.Property(r => r.EndLocation)
            .HasColumnName("end_location");

        builder.Property(r => r.RouteGeometry)
            .HasColumnName("route_geometry");

        builder.Property(r => r.EncodedPolyline)
            .HasColumnName("encoded_polyline");

        builder.Property(r => r.DistanceKm)
            .HasColumnName("distance_km")
            .HasColumnType("numeric(10,2)");

        builder.Property(r => r.EstimatedTimeMin)
            .HasColumnName("estimated_time_min");

        builder.Property(r => r.Co2SavedKg)
            .HasColumnName("co2_saved_kg")
            .HasColumnType("numeric(10,4)");

        builder.Property(r => r.EstimatedCalories)
            .HasColumnName("estimated_calories")
            .HasColumnType("numeric(8,2)");

        builder.Property(r => r.DifficultyLevel)
            .HasColumnName("difficulty_level");

        builder.Property(r => r.MapData)
            .HasColumnName("map_data")
            .HasColumnType("jsonb");

        builder.Property(r => r.PhotoUrl)
            .HasColumnName("photo_url")
            .HasMaxLength(500);

        builder.Property(r => r.AvailableDate)
            .HasColumnName("available_date");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        // Restricciones espejo del DDL
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "chk_routes_distance_km",
                "distance_km IS NULL OR distance_km >= 0");

            t.HasCheckConstraint(
                "chk_routes_estimated_time_min",
                "estimated_time_min IS NULL OR estimated_time_min >= 0");

            t.HasCheckConstraint(
                "chk_routes_difficulty_level",
                "difficulty_level IS NULL OR difficulty_level BETWEEN 1 AND 5");

            t.HasCheckConstraint(
                "chk_routes_map_data",
                "map_data IS NULL OR jsonb_typeof(map_data) = 'object'");
        });

        builder.HasMany(r => r.RoutePois)
            .WithOne(rp => rp.Route)
            .HasForeignKey(rp => rp.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
