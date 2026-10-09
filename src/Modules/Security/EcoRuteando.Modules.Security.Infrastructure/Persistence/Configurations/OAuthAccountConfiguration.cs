using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;

public sealed class OAuthAccountConfiguration : IEntityTypeConfiguration<OAuthAccount>
{
    public void Configure(EntityTypeBuilder<OAuthAccount> builder)
    {
        builder.ToTable("oauth_accounts", "security");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.Provider)
            .HasColumnName("provider")
            .HasColumnType("oauth_provider")
            .IsRequired();

        builder.Property(o => o.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.EncryptedAccessToken)
            .HasColumnName("encrypted_access_token");

        builder.Property(o => o.EncryptedRefreshToken)
            .HasColumnName("encrypted_refresh_token");

        builder.Property(o => o.TokenScope)
            .HasColumnName("token_scope")
            .HasMaxLength(255);

        builder.Property(o => o.TokenExpiresAt)
            .HasColumnName("token_expires_at");

        builder.Property(o => o.OAuthEmail)
            .HasColumnName("oauth_email")
            .HasMaxLength(150);

        builder.Property(o => o.OAuthName)
            .HasColumnName("oauth_name")
            .HasMaxLength(200);

        builder.Property(o => o.OAuthPhotoUrl)
            .HasColumnName("oauth_photo_url")
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.Provider, o.ExternalId })
            .IsUnique();

        builder.HasIndex(o => new { o.UserId, o.Provider })
            .IsUnique();
    }
}
