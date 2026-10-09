using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetObstacleReportsForValidation;

/// <summary>
/// Consulta administrativa de reportes pendientes de validación
/// (opcionalmente filtrados por estado).
/// </summary>
public sealed record GetObstacleReportsForValidationQuery(
    ReportStatus? Status = null)
    : IRequest<IReadOnlyList<ObstacleReportResponse>>;