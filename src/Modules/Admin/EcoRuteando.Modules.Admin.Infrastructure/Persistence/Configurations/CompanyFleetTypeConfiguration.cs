using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public sealed class CompanyFleetTypeConfiguration : IEntityTypeConfiguration<CompanyFleetType>
{
    public void Configure(EntityTypeBuilder<CompanyFleetType> builder)
    {
        builder.ToTable("company_fleet_types", "admin");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(f => f.VehicleType)
            .HasColumnName("vehicle_type")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(f => f.FuelType)
            .HasColumnName("fuel_type")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(f => f.EmissionFactorKgKm)
            .HasColumnName("emission_factor_kg_km")
            .HasPrecision(8, 6)
            .IsRequired();

        builder.Property(f => f.SourceReference)
            .HasColumnName("source_reference")
            .HasMaxLength(150)
            .HasDefaultValue("UPME / MinAmbiente Colombia")
            .IsRequired();

        builder.Property(f => f.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(f => f.ValidFrom)
            .HasColumnName("valid_from")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(f => f.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
