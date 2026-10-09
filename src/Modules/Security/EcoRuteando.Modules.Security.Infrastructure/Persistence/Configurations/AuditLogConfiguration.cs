using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs", "security");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasColumnName("entity_name").HasMaxLength(80);
        builder.Property(a => a.EntityId).HasColumnName("entity_id").HasMaxLength(100);
        builder.Property(a => a.BeforeData).HasColumnName("before_data").HasColumnType("jsonb");
        builder.Property(a => a.AfterData).HasColumnName("after_data").HasColumnType("jsonb");
        builder.Property(a => a.SourceIp).HasColumnName("source_ip");
        builder.Property(a => a.UserAgent).HasColumnName("user_agent");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Action);
    }
}
