namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Ruta marcada como favorita por un usuario
/// (tabla mobility.favorite_routes con PK compuesta user_id + route_id).
/// Una misma ruta no puede duplicarse en la lista de favoritos de un usuario.
/// </summary>
public sealed class FavoriteRoute
{
    public Guid UserId { get; private set; }

    public Guid RouteId { get; private set; }

    /// <summary>
    /// Etiqueta opcional que el usuario puede asignar al favorito.
    /// </summary>
    public string? Label { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Route Route { get; private set; } = null!;

    private FavoriteRoute()
    {
    }

    public FavoriteRoute(Guid userId, Guid routeId, string? label = null)
    {
        UserId = userId;
        RouteId = routeId;
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
