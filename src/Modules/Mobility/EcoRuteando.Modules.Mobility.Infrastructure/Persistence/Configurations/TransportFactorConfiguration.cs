using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class TransportFactorConfiguration
    : IEntityTypeConfiguration<TransportFactor>
{
    public void Configure(EntityTypeBuilder<TransportFactor> builder)
    {
        builder.ToTable("transport_factors", "admin");

        builder.HasKey(tf => tf.Id);

        builder.Property(tf => tf.Id)
            .HasColumnName("id");

        builder.Property(tf => tf.TransportType)
            .HasColumnName("transport_type")
            .HasColumnType("transport_type")
            .IsRequired();

        builder.Property(tf => tf.Co2FactorKgKm)
            .HasColumnName("co2_factor_kg_km")
            .HasColumnType("numeric(8,6)")
            .IsRequired();

        builder.Property(tf => tf.CalorieFactorKm)
            .HasColumnName("calorie_factor_km")
            .HasColumnType("numeric(6,2)");

        builder.Property(tf => tf.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(tf => tf.ValidUntil)
            .HasColumnName("valid_until");

        builder.HasIndex(tf => tf.TransportType)
            .HasDatabaseName("ix_transport_factors_transport_type");
    }
}
