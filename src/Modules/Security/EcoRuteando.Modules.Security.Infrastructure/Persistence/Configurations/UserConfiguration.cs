using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", "security");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(u => u.FirstName)
                   .HasColumnName("first_name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .HasColumnName("last_name")
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasColumnType("citext")
                   .IsRequired();

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .HasMaxLength(255);

            builder.Property(u => u.PhoneNumber)
                   .HasColumnName("phone_number")
                   .HasMaxLength(30);

            builder.Property(u => u.ProfilePhotoUrl)
                   .HasColumnName("profile_photo_url")
                   .HasMaxLength(500);

            builder.Property(u => u.AcceptedTerms)
                   .HasColumnName("accepted_terms")
                   .HasDefaultValue(false);

            builder.Property(u => u.TermsAcceptedAt)
                   .HasColumnName("terms_accepted_at");

            builder.Property(u => u.EmailVerified)
                   .HasColumnName("email_verified")
                   .HasDefaultValue(false);

            // Map CLR enums directly to PostgreSQL enum types (no string conversion)
            builder.Property(u => u.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .IsRequired();

            builder.Property(u => u.IsGuest)
                   .HasColumnName("is_guest")
                   .HasDefaultValue(false);

            builder.Property(u => u.PreferredLanguage)
                    .HasColumnName("preferred_language")
                    .HasMaxLength(10)
                    .IsRequired();

            builder.Property(u => u.UiTheme)
                       .HasColumnName("ui_theme")
                       .HasMaxLength(20)
                       .IsRequired();

            builder.Property(u => u.PrimaryRoleId)
                   .HasColumnName("primary_role_id");

            builder.Property(u => u.FailedAttempts)
                   .HasColumnName("failed_attempts")
                   .HasDefaultValue(0);

            builder.Property(u => u.LockedUntil)
                   .HasColumnName("locked_until");

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("now()");

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasDefaultValueSql("now()");

            builder.HasOne(u => u.PrimaryRole)
                   .WithMany()
                   .HasForeignKey(u => u.PrimaryRoleId);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_users_phone_number",
                    "phone_number ~ '^\\+?[0-9 ()-]{6,30}$'");
            });
        }
    }
}