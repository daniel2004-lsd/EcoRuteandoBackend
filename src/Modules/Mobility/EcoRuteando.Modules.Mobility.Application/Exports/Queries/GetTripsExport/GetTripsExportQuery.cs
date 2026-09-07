using EcoRuteando.Modules.Mobility.Application.Exports;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetTripsExport;

/// <summary>
/// Exporta el historial de trayectos del usuario autenticado (CU19).
/// From/To son opcionales (RF29.2): si se indican, solo se exportan los
/// trayectos iniciados dentro del período (From inclusive, To inclusive).
/// </summary>
public sealed record GetTripsExportQuery(
    Guid UserId,
    ExportFormat Format,
    DateTime? From = null,
    DateTime? To = null)
    : IRequest<ExportFileResponse>;