using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class ObstacleReportConfiguration : IEntityTypeConfiguration<ObstacleReport>
{
    public void Configure(EntityTypeBuilder<ObstacleReport> builder)
    {
        builder.ToTable("obstacle_reports", "community");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.ReportType)
            .HasColumnName("report_type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasColumnType("report_status")
            .IsRequired();

        // Columna PostGIS GEOGRAPHY(Point, 4326)
        builder.Property(r => r.Location)
            .HasColumnName("location")
            .IsRequired();

        builder.Property(r => r.AddressText)
            .HasColumnName("address_text")
            .HasMaxLength(300);

        builder.Property(r => r.PhotoUrl)
            .HasColumnName("photo_url")
            .HasMaxLength(500);

        builder.Property(r => r.ValidatorId)
            .HasColumnName("validator_id");

        builder.Property(r => r.ValidationNote)
            .HasColumnName("validation_note");

        builder.Property(r => r.ValidatedAt)
            .HasColumnName("validated_at");

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        // Espejo del DDL: el usuario no puede validar su propio reporte.
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "obstacle_reports_validator_check",
                "validator_id IS NULL OR validator_id <> user_id");
        });
    }
}