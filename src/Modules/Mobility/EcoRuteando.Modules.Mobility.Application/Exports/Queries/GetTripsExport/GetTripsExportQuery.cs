using EcoRuteando.Modules.Mobility.Application.Exports;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetTripsExport;

/// <summary>
/// Exporta el historial de trayectos del usuario autenticado (CU19).
/// </summary>
public sealed record GetTripsExportQuery(
    Guid UserId,
    ExportFormat Format)
    : IRequest<ExportFileResponse>;