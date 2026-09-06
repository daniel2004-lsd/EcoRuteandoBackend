using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Statistics.Queries.GetStats;

/// <summary>
/// Estadísticas del sistema para el portal de administración (CU08).
/// El rango de fechas es opcional; sin él se agrega todo el histórico.
/// </summary>
public sealed record GetStatsQuery(
    DateTime? From = null,
    DateTime? To = null)
    : IRequest<GetStatsResponse>;