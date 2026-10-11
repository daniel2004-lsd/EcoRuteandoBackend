using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
{
    public void Configure(EntityTypeBuilder<SupportMessage> builder)
    {
        builder.ToTable("support_messages", "admin");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.TicketId)
            .HasColumnName("ticket_id")
            .IsRequired();

        builder.Property(m => m.SenderId)
            .HasColumnName("sender_id");

        builder.Property(m => m.IsAdmin)
            .HasColumnName("is_admin")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(m => m.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(m => m.AttachmentUrl)
            .HasColumnName("attachment_url")
            .HasMaxLength(500);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
