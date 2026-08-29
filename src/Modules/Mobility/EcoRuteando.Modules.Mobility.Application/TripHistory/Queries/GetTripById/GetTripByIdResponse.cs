using System.Text.Json;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripById;

/// <summary>
/// Punto geográfico del recorrido real (WGS84).
/// </summary>
public sealed record TripGeoPointResponse(
    double Latitude,
    double Longitude);

/// <summary>
/// Detalle de un trayecto del historial para la HU de visualización:
/// incluye las métricas eco reales y el recorrido vivido (actual_route)
/// como lista de coordenadas para dibujar en el mapa.
/// </summary>
public sealed record GetTripByIdResponse(
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
    IReadOnlyList<TripGeoPointResponse>? ActualRoute,
    JsonDocument? GpsData)
{
    /// <summary>
    /// Convierte el LINESTRING de geo (actual_route) a lista de puntos.
    /// En NetTopologySuite: X = longitud, Y = latitud.
    /// </summary>
    public static IReadOnlyList<TripGeoPointResponse>? ToRoutePoints(LineString? lineString)
    {
        if (lineString is null)
            return null;

        var points = new List<TripGeoPointResponse>(lineString.NumPoints);
        foreach (var coordinate in lineString.Coordinates)
        {
            points.Add(new TripGeoPointResponse(coordinate.Y, coordinate.X));
        }

        return points;
    }
}
