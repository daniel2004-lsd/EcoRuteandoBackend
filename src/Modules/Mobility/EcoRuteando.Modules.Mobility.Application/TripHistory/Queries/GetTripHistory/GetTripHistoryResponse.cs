namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripHistory;

/// <summary>
/// Ítem del historial de trayectos de un usuario.
/// </summary>
public sealed record GetTripHistoryResponse(
    Guid UsageId,
    Guid RouteId,
    string RouteName,
    string? TransportMode,
    string Source,
    DateTime StartedAt,
    DateTime? EndedAt,
    bool Completed,
    decimal? ActualDistanceKm,
    int? ActualDurationMin,
    decimal? ActualCo2Kg,
    decimal? EstimatedCalories);
