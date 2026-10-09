namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPoiById;

/// <summary>
/// Detalle completo de un punto de interés.
/// </summary>
public sealed record GetPoiByIdResponse(
    Guid Id,
    string Name,
    string PoiType,
    string? Description,
    double Lat,
    double Lng,
    string? Address,
    string? IconUrl,
    bool IsActive,
    string? Source,
    DateTime CreatedAt,
    DateTime? UpdatedAt);