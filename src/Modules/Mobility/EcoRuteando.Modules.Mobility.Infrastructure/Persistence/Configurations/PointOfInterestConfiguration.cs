using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class PointOfInterestConfiguration : IEntityTypeConfiguration<PointOfInterest>
{
    public void Configure(EntityTypeBuilder<PointOfInterest> builder)
    {
        builder.ToTable("points_of_interest", "mobility");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.PoiType)
            .HasColumnName("poi_type")
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(p => p.Description)
            .HasColumnName("description");

        // Columna PostGIS GEOGRAPHY(Point, 4326)
        builder.Property(p => p.Location)
            .HasColumnName("location")
            .IsRequired();

        builder.Property(p => p.Address)
            .HasColumnName("address")
            .HasMaxLength(255);

        builder.Property(p => p.IconUrl)
            .HasColumnName("icon_url")
            .HasMaxLength(500);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.Source)
            .HasColumnName("source")
            .HasMaxLength(100);

        builder.Property(p => p.ExternalData)
            .HasColumnName("external_data")
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "chk_pois_external_data",
                "external_data IS NULL OR jsonb_typeof(external_data) = 'object'");
        });

        builder.HasMany(p => p.RoutePois)
            .WithOne(rp => rp.PointOfInterest)
            .HasForeignKey(rp => rp.PoiId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
