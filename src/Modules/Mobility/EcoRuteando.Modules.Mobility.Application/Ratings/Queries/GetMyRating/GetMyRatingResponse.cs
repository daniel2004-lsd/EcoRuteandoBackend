namespace EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetMyRating;

/// <summary>
/// Calificación que el usuario autenticado ya asignó a una ruta
/// (null si aún no la ha calificado).
/// </summary>
public sealed record GetMyRatingResponse(
    Guid Id,
    Guid RouteId,
    short RatingValue,
    string? Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt);