using EcoRuteando.Modules.Mobility.Application.Exports;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetStatsExport;

/// <summary>
/// Exporta las estadísticas del portal de administración (CU08/CU19)
/// con filtro opcional por rango de fechas.
/// </summary>
public sealed record GetStatsExportQuery(
    DateTime? From,
    DateTime? To,
    ExportFormat Format)
    : IRequest<ExportFileResponse>;