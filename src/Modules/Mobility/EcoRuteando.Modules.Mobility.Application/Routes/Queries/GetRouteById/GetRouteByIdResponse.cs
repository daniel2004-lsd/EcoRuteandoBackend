using System.Text.Json;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRouteById;

/// <summary>
/// Punto geográfico para pintar en el mapa (WGS84).
/// </summary>
public sealed record GeoPointResponse(
    double Latitude,
    double Longitude);

public sealed record RoutePoiResponse(
    Guid Id,
    string Name,
    string PoiType,
    string? Description,
    double Latitude,
    double Longitude,
    string? Address,
    string? IconUrl);

/// <summary>
/// Detalle completo de una ruta para la HU de visualización:
/// incluye la polyline codificada (para dibujar el trazado),
/// los puntos de interés ordenados por sort_order y las métricas eco.
/// </summary>
public sealed record GetRouteByIdResponse(
    Guid Id,
    string Name,
    string? Description,
    string TransportType,
    string Status,
    string StartName,
    string DestinationName,
    GeoPointResponse? StartLocation,
    GeoPointResponse? EndLocation,
    string? EncodedPolyline,
    decimal? DistanceKm,
    int? EstimatedTimeMin,
    decimal? Co2SavedKg,
    decimal? EstimatedCalories,
    short? DifficultyLevel,
    JsonDocument? MapData,
    string? PhotoUrl,
    DateOnly? AvailableDate,
    IReadOnlyList<RoutePoiResponse> Pois);
