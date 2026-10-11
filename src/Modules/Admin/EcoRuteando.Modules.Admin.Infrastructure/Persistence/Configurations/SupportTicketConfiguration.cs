using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("support_tickets", "admin");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.UserId)
            .HasColumnName("user_id");

        builder.Property(t => t.AdminId)
            .HasColumnName("admin_id");

        builder.Property(t => t.Subject)
            .HasColumnName("subject")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v == SupportTicketStatus.Pending ? "pending" :
                     v == SupportTicketStatus.InProgress ? "in_progress" :
                     v == SupportTicketStatus.Resolved ? "resolved" :
                     v == SupportTicketStatus.Closed ? "closed" : "pending",
                v => v == "pending" ? SupportTicketStatus.Pending :
                     v == "in_progress" ? SupportTicketStatus.InProgress :
                     v == "resolved" ? SupportTicketStatus.Resolved :
                     v == "closed" ? SupportTicketStatus.Closed : SupportTicketStatus.Pending)
            .HasColumnType("admin.support_status")
            .HasDefaultValue(SupportTicketStatus.Pending)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasDefaultValue((short)2)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(t => t.ClosedAt)
            .HasColumnName("closed_at");

        builder.HasMany(t => t.Messages)
            .WithOne(m => m.Ticket)
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
