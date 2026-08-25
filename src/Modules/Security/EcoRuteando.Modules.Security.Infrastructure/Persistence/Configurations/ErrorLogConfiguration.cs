using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;

public sealed class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs", "security");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.ErrorLevel)
            .HasColumnName("error_level")
            .HasColumnType("error_level")
            .IsRequired();
        builder.Property(e => e.Source).HasColumnName("source").HasMaxLength(200);
        builder.Property(e => e.Message).HasColumnName("message").IsRequired();
        builder.Property(e => e.StackTrace).HasColumnName("stack_trace");
        builder.Property(e => e.ContextData).HasColumnName("context_data").HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ErrorLevel);
    }
}
