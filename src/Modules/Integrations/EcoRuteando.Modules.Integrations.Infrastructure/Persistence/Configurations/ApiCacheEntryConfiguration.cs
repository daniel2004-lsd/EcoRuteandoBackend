using EcoRuteando.Modules.Integrations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Configurations;

public class ApiCacheEntryConfiguration : IEntityTypeConfiguration<ApiCacheEntry>
{
    public void Configure(EntityTypeBuilder<ApiCacheEntry> builder)
    {
        builder.ToTable("api_cache", "integrations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Provider)
            .HasColumnName("provider")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(c => c.Endpoint)
            .HasColumnName("endpoint")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Parameters)
            .HasColumnName("parameters")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(c => c.ParametersHash)
            .HasColumnName("parameters_hash")
            .HasMaxLength(64)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(c => c.Response)
            .HasColumnName("response")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(c => c.IsValid)
            .HasColumnName("is_valid")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.HasIndex(c => new { c.Provider, c.Endpoint, c.ParametersHash });
    }
}
