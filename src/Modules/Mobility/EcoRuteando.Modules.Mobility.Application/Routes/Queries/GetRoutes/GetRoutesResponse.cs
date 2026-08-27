namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRoutes;

/// <summary>
/// Tarjeta resumen de ruta para el catálogo/listado.
/// </summary>
public sealed record GetRoutesResponse(
    Guid Id,
    string Name,
    string TransportType,
    string Status,
    string StartName,
    string DestinationName,
    decimal? DistanceKm,
    int? EstimatedTimeMin,
    decimal? Co2SavedKg,
    short? DifficultyLevel,
    string? PhotoUrl);
