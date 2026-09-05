using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Calificación que un usuario asigna a una ruta tras completar un trayecto
/// (tabla mobility.ratings, con UNIQUE(user_id, route_id)).
/// La puntuación va de 1 a 5 estrellas y el comentario es opcional.
/// </summary>
public sealed class Rating : Entity<Guid>
{
    public Guid UserId { get; private set; }

    public Guid RouteId { get; private set; }

    /// <summary>
    /// Trayecto (route_usage) asociado a la valoración, si aplica.
    /// </summary>
    public long? UsageId { get; private set; }

    public short RatingValue { get; private set; }

    public string? Comment { get; private set; }

    public int HelpfulCount { get; private set; }

    public bool IsPublished { get; private set; }

    public Route? Route { get; private set; }

    private Rating()
    {
    }

    public Rating(
        Guid userId,
        Guid routeId,
        short ratingValue,
        string? comment,
        long? usageId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (routeId == Guid.Empty)
            throw new DomainException("La ruta es obligatoria.");

        if (ratingValue is < 1 or > 5)
            throw new DomainException("La calificación debe estar entre 1 y 5 estrellas.");

        Id = Guid.NewGuid();
        UserId = userId;
        RouteId = routeId;
        UsageId = usageId;
        RatingValue = ratingValue;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        HelpfulCount = 0;
        IsPublished = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza una calificación existente (el usuario puede cambiar su valoración).
    /// </summary>
    public void Update(
        short ratingValue,
        string? comment)
    {
        if (ratingValue is < 1 or > 5)
            throw new DomainException("La calificación debe estar entre 1 y 5 estrellas.");

        RatingValue = ratingValue;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
