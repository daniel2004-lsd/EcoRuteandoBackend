using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions", "security");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(64).IsRequired();
        builder.Property(s => s.SourceIp).HasColumnName("source_ip");
        builder.Property(s => s.UserAgent).HasColumnName("user_agent");
        builder.Property(s => s.Device).HasColumnName("device").HasMaxLength(100);
        builder.Property(s => s.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.LastUsedAt).HasColumnName("last_used_at").IsRequired();
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(s => s.RevokedAt).HasColumnName("revoked_at");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.RefreshTokenHash).IsUnique();
        builder.HasIndex(s => new { s.UserId, s.IsActive });
    }
}
