namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPois;

/// <summary>
/// Tarjeta resumen de un punto de interés para el listado/mapa.
/// </summary>
public sealed record GetPoisResponse(
    Guid Id,
    string Name,
    string PoiType,
    string? Description,
    double Lat,
    double Lng,
    string? Address,
    string? IconUrl);