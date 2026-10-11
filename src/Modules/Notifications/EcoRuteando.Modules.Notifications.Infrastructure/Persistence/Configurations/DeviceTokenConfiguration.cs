using EcoRuteando.Modules.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Notifications.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens", "notifications");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(d => d.Platform)
            .HasColumnName("platform")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(d => d.ExpoPushToken)
            .HasColumnName("expo_push_token")
            .IsRequired();

        builder.Property(d => d.DeviceId)
            .HasColumnName("device_id")
            .HasMaxLength(255);

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(d => d.LastUsedAt)
            .HasColumnName("last_used_at");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(d => new { d.UserId, d.Platform })
            .IsUnique()
            .HasDatabaseName("uq_device_tokens_user_platform");
    }
}
