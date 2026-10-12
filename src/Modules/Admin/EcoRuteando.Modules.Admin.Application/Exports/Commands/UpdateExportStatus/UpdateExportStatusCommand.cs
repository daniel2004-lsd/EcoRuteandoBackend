using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Commands.UpdateExportStatus;

public record UpdateExportStatusCommand(
    Guid Id,
    ExportStatus Status,
    string? FileUrl = null,
    long? SizeBytes = null,
    int? RecordsCount = null,
    string? ErrorMessage = null) : IRequest<AdminExportDto>;
