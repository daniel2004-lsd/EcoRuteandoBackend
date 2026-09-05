using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Application.Statistics.Queries.GetStats;

/// <summary>
/// Ítem del desglose por modo de transporte (CU08).
/// </summary>
public sealed record TransportModeStatResponse(
    string? Mode,
    int Trips,
    decimal Co2Kg,
    int Users);

/// <summary>
/// Ítem del desglose mensual (CU08).
/// </summary>
public sealed record MonthStatResponse(
    string Period,
    int Trips,
    decimal Co2Kg);

/// <summary>
/// Respuesta del portal de estadísticas.
/// </summary>
public sealed record GetStatsResponse(
    int TotalRoutesConsulted,
    int ActiveUsers,
    decimal TotalCo2SavedKg,
    int CompletedTrips,
    double AverageDistanceKm,
    IReadOnlyList<TransportModeStatResponse> ByTransportMode,
    IReadOnlyList<MonthStatResponse> Monthly);