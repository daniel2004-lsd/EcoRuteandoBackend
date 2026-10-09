namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Entidad de unión entre rutas y puntos de interés
/// (tabla mobility.route_poi con PK compuesta route_id + poi_id).
/// </summary>
public sealed class RoutePoi
{
    public Guid RouteId { get; private set; }

    public Guid PoiId { get; private set; }

    /// <summary>
    /// Orden de visita del POI dentro de la ruta.
    /// </summary>
    public short? SortOrder { get; private set; }

    public Route Route { get; private set; } = null!;

    public PointOfInterest PointOfInterest { get; private set; } = null!;

    private RoutePoi()
    {
    }

    public RoutePoi(Guid routeId, Guid poiId, short? sortOrder = null)
    {
        RouteId = routeId;
        PoiId = poiId;
        SortOrder = sortOrder;
    }
}
