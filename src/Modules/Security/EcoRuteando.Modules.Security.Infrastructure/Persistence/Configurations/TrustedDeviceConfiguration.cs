using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;

public sealed class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.ToTable("trusted_devices", "security");

        builder.HasKey(td => td.Id);

        builder.Property(td => td.Id).HasColumnName("id");
        builder.Property(td => td.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(td => td.DeviceName).HasColumnName("device_name").HasMaxLength(100);
        builder.Property(td => td.IsRevoked).HasColumnName("is_revoked").IsRequired();
        builder.Property(td => td.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(td => td.UpdatedAt).HasColumnName("updated_at");
        builder.Property(td => td.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(td => td.RevokedAt).HasColumnName("revoked_at");

        builder.HasOne(td => td.User)
            .WithMany()
            .HasForeignKey(td => td.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(td => new { td.UserId, td.IsRevoked });
    }
}
