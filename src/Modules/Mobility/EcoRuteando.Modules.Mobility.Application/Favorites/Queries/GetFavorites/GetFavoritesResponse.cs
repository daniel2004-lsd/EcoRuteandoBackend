namespace EcoRuteando.Modules.Mobility.Application.Favorites.Queries.GetFavorites;

/// <summary>
/// Ítem de la lista de rutas favoritas de un usuario.
/// </summary>
public sealed record GetFavoritesResponse(
    Guid RouteId,
    string RouteName,
    string? Description,
    string TransportType,
    string? Label,
    DateTime CreatedAt,
    decimal? DistanceKm,
    int? EstimatedTimeMin,
    decimal? Co2SavedKg,
    short? DifficultyLevel,
    decimal? StartLat,
    decimal? StartLng,
    decimal? EndLat,
    decimal? EndLng);
