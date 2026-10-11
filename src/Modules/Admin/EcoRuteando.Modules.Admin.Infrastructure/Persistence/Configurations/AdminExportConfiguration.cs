using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public class AdminExportConfiguration : IEntityTypeConfiguration<AdminExport>
{
    public void Configure(EntityTypeBuilder<AdminExport> builder)
    {
        builder.ToTable("exports", "admin");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.ExportType)
            .HasColumnName("export_type")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(e => e.Format)
            .HasColumnName("format")
            .HasConversion(
                v => v == ExportFormat.Pdf ? "PDF" :
                     v == ExportFormat.Xlsx ? "XLSX" :
                     v == ExportFormat.Csv ? "CSV" :
                     v == ExportFormat.Json ? "JSON" : "PDF",
                v => v == "PDF" ? ExportFormat.Pdf :
                     v == "XLSX" ? ExportFormat.Xlsx :
                     v == "CSV" ? ExportFormat.Csv :
                     v == "JSON" ? ExportFormat.Json : ExportFormat.Pdf)
            .HasColumnType("admin.export_format")
            .IsRequired();

        builder.Property(e => e.Filters)
            .HasColumnName("filters")
            .HasColumnType("jsonb");

        builder.Property(e => e.FileUrl)
            .HasColumnName("file_url")
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v == ExportStatus.Generating ? "generating" :
                     v == ExportStatus.Ready ? "ready" :
                     v == ExportStatus.Error ? "error" : "generating",
                v => v == "generating" ? ExportStatus.Generating :
                     v == "ready" ? ExportStatus.Ready :
                     v == "error" ? ExportStatus.Error : ExportStatus.Generating)
            .HasColumnType("admin.export_status")
            .HasDefaultValue(ExportStatus.Generating)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnType("text");

        builder.Property(e => e.SizeBytes)
            .HasColumnName("size_bytes");

        builder.Property(e => e.RecordsCount)
            .HasColumnName("records_count");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at");
    }
}
