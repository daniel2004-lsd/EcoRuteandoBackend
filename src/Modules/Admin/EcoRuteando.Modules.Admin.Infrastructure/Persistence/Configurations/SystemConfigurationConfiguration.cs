using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("system_configuration", "admin");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasDefaultValue((short)1);

        builder.Property(c => c.AppName)
            .HasColumnName("app_name")
            .HasMaxLength(100)
            .HasDefaultValue("EcoRuteando")
            .IsRequired();

        builder.Property(c => c.AdminEmail)
            .HasColumnName("admin_email")
            .HasMaxLength(150);

        builder.Property(c => c.SupportPhone)
            .HasColumnName("support_phone")
            .HasMaxLength(30);

        builder.Property(c => c.TermsVersion)
            .HasColumnName("terms_version")
            .HasMaxLength(20);

        builder.Property(c => c.TermsUrl)
            .HasColumnName("terms_url")
            .HasMaxLength(500);

        builder.Property(c => c.PrivacyVersion)
            .HasColumnName("privacy_version")
            .HasMaxLength(20);

        builder.Property(c => c.PrivacyUrl)
            .HasColumnName("privacy_url")
            .HasMaxLength(500);

        builder.Property(c => c.EnabledApis)
            .HasColumnName("enabled_apis")
            .HasColumnType("jsonb");

        builder.Property(c => c.MapCacheTtlSec)
            .HasColumnName("map_cache_ttl_sec")
            .HasDefaultValue(3600)
            .IsRequired();

        builder.Property(c => c.WeatherCacheTtlSec)
            .HasColumnName("weather_cache_ttl_sec")
            .HasDefaultValue(1800)
            .IsRequired();

        builder.Property(c => c.PrimaryColor)
            .HasColumnName("primary_color")
            .HasMaxLength(7)
            .HasDefaultValue("#2E7D32")
            .IsRequired();

        builder.Property(c => c.SecondaryColor)
            .HasColumnName("secondary_color")
            .HasMaxLength(7)
            .HasDefaultValue("#66BB6A")
            .IsRequired();

        builder.Property(c => c.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("updated_by");
    }
}
