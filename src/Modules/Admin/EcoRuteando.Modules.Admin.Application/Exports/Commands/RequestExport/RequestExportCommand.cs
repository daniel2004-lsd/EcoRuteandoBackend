using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Commands.RequestExport;

public record RequestExportCommand(
    Guid? UserId,
    string ExportType,
    ExportFormat Format,
    string? Filters = null) : IRequest<AdminExportDto>;
