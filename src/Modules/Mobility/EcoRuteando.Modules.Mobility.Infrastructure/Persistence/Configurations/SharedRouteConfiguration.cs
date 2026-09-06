using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class SharedRouteConfiguration
    : IEntityTypeConfiguration<SharedRoute>
{
    public void Configure(EntityTypeBuilder<SharedRoute> builder)
    {
        builder.ToTable("shared_routes", "mobility");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Id)
            .HasColumnName("id");

        builder.Property(sr => sr.UsageId)
            .HasColumnName("usage_id")
            .IsRequired();

        builder.Property(sr => sr.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(sr => sr.SocialNetwork)
            .HasColumnName("social_network")
            .HasMaxLength(50);

        builder.Property(sr => sr.SharedData)
            .HasColumnName("shared_data")
            .HasColumnType("jsonb");

        builder.Property(sr => sr.Confirmed)
            .HasColumnName("confirmed")
            .HasDefaultValue(true);

        builder.Property(sr => sr.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");
    }
}