namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Queries.GetSharedRoutes;

/// <summary>
/// Ítem de la lista de recorridos compartidos por un usuario.
/// </summary>
public sealed record GetSharedRoutesResponse(
    Guid Id,
    Guid UsageId,
    string? SocialNetwork,
    bool Confirmed,
    DateTime CreatedAt,
    decimal? ActualDistanceKm,
    int? ActualDurationMin,
    decimal? ActualCo2Kg,
    string? RouteName,
    string? StartName,
    string? DestinationName);