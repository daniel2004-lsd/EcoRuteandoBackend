using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Dtos;

public record AdminExportDto(
    Guid Id,
    Guid? UserId,
    string ExportType,
    ExportFormat Format,
    string FormatName,
    string? Filters,
    string? FileUrl,
    ExportStatus Status,
    string StatusName,
    string? ErrorMessage,
    long? SizeBytes,
    int? RecordsCount,
    DateTime CreatedAt,
    DateTime? ExpiresAt);
