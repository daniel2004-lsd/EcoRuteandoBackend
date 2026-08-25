using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;

public sealed class SecurityPolicyConfiguration : IEntityTypeConfiguration<SecurityPolicy>
{
    public void Configure(EntityTypeBuilder<SecurityPolicy> builder)
    {
        builder.ToTable("security_policies", "security");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.MinPasswordLength)
            .HasColumnName("min_password_length")
            .IsRequired()
            .HasDefaultValue(8);

        builder.Property(p => p.RequireUppercase)
            .HasColumnName("require_uppercase")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.RequireNumbers)
            .HasColumnName("require_numbers")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.RequireSpecialChars)
            .HasColumnName("require_special_chars")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.PasswordExpirationDays)
            .HasColumnName("password_expiration_days")
            .IsRequired()
            .HasDefaultValue(90);

        builder.Property(p => p.MaxFailedAttempts)
            .HasColumnName("max_failed_attempts")
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(p => p.LockoutTimeMinutes)
            .HasColumnName("lockout_time_minutes")
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(p => p.MaxActiveSessions)
            .HasColumnName("max_active_sessions")
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CHK_security_policies_singleton",
                "id = 1");
        });
    }
}
