using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public class TransportFactorConfiguration : IEntityTypeConfiguration<TransportFactor>
{
    public void Configure(EntityTypeBuilder<TransportFactor> builder)
    {
        builder.ToTable("transport_factors", "admin");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.TransportType)
            .HasColumnName("transport_type")
            .HasConversion(
                v => v == TransportType.Bike ? "bike" :
                     v == TransportType.PublicTransport ? "public_transport" :
                     v == TransportType.Mixed ? "mixed" :
                     v == TransportType.Walking ? "walking" :
                     v == TransportType.Car ? "car" : "bike",
                v => v == "bike" ? TransportType.Bike :
                     v == "public_transport" ? TransportType.PublicTransport :
                     v == "mixed" ? TransportType.Mixed :
                     v == "walking" ? TransportType.Walking :
                     v == "car" ? TransportType.Car : TransportType.Bike)
            .HasColumnType("mobility.transport_type")
            .IsRequired();

        builder.Property(f => f.Co2FactorKgKm)
            .HasColumnName("co2_factor_kg_km")
            .HasPrecision(8, 6)
            .IsRequired();

        builder.Property(f => f.CalorieFactorKm)
            .HasColumnName("calorie_factor_km")
            .HasPrecision(6, 2);

        builder.Property(f => f.ValidFrom)
            .HasColumnName("valid_from")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(f => f.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(f => f.Source)
            .HasColumnName("source")
            .HasMaxLength(200);

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
