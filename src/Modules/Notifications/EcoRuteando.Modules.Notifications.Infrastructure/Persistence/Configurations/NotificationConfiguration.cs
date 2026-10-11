using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Notifications.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", "notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(n => n.UserId);

        builder.Property(n => n.NotificationType)
            .HasColumnName("notification_type")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<NotificationType>(v, true))
            .HasColumnType("notifications.notification_type")
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(n => n.ActionUrl)
            .HasColumnName("action_url")
            .HasMaxLength(500);

        builder.Property(n => n.ExtraData)
            .HasColumnName("extra_data")
            .HasColumnType("jsonb");

        builder.Property(n => n.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.ReadAt)
            .HasColumnName("read_at");

        builder.Property(n => n.Channel)
            .HasColumnName("channel")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<NotificationChannel>(v, true))
            .HasColumnType("notifications.notification_channel")
            .HasDefaultValue(NotificationChannel.InApp)
            .IsRequired();

        builder.Property(n => n.IsSent)
            .HasColumnName("is_sent")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at");

        builder.Property(n => n.TemplateId)
            .HasColumnName("template_id");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(n => n.Template)
            .WithMany()
            .HasForeignKey(n => n.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
