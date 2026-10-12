using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Domain.Entities;

public class AdminExport
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string ExportType { get; private set; } = string.Empty;
    public ExportFormat Format { get; private set; }
    public string? Filters { get; private set; }
    public string? FileUrl { get; private set; }
    public ExportStatus Status { get; private set; } = ExportStatus.Generating;
    public string? ErrorMessage { get; private set; }
    public long? SizeBytes { get; private set; }
    public int? RecordsCount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }

    private AdminExport() { }

    public static AdminExport Create(
        Guid? userId,
        string exportType,
        ExportFormat format,
        string? filters = null)
    {
        if (string.IsNullOrWhiteSpace(exportType))
            throw new ArgumentException("ExportType cannot be empty.", nameof(exportType));

        return new AdminExport
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExportType = exportType.Trim(),
            Format = format,
            Filters = filters,
            Status = ExportStatus.Generating,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Complete(string fileUrl, long sizeBytes, int recordsCount, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("FileUrl cannot be empty.", nameof(fileUrl));

        FileUrl = fileUrl.Trim();
        SizeBytes = Math.Max(0, sizeBytes);
        RecordsCount = Math.Max(0, recordsCount);
        Status = ExportStatus.Ready;
        ErrorMessage = null;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7);
    }

    public void Fail(string errorMessage)
    {
        Status = ExportStatus.Error;
        ErrorMessage = errorMessage;
    }
}
