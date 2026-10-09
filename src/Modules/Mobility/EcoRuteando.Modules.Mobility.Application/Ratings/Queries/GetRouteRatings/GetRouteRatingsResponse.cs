namespace EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetRouteRatings;

/// <summary>
/// Calificación individual visible en la lista de valoraciones de una ruta.
/// </summary>
public sealed record RouteRatingResponse(
    Guid Id,
    Guid UserId,
    short RatingValue,
    string? Comment,
    DateTime CreatedAt);

/// <summary>
/// Resumen de valoraciones de una ruta: lista de reseñas, puntuación
/// promedio y total de calificaciones.
/// </summary>
public sealed record GetRouteRatingsResponse(
    Guid RouteId,
    IReadOnlyList<RouteRatingResponse> Ratings,
    decimal? AverageRating,
    int TotalCount);