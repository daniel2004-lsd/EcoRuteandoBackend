using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetMyObstacleReports;

/// <summary>
/// Reportes de obstáculo del usuario autenticado (CU07: el ciudadano consulta
/// el estado de sus reportes).
/// </summary>
public sealed record GetMyObstacleReportsQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<ObstacleReportResponse>>;