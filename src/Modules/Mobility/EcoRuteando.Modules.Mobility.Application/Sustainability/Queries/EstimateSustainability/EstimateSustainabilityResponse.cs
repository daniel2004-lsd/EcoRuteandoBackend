namespace EcoRuteando.Modules.Mobility.Application.Sustainability.Queries.EstimateSustainability;

/// <summary>
/// Resultado de la estimación de sostenibilidad para una consulta de ruta.
/// Distancia y duración provienen de Google Maps (datos reales); el CO₂ del modo,
/// el CO₂ ahorrado (frente al vehículo particular) y las calorías se calculan
/// aplicando los factores configurados en admin.transport_factors.
/// </summary>
public sealed record EstimateSustainabilityResponse(
    string TransportType,
    decimal DistanceKm,
    int EstimatedTimeMin,
    decimal? Co2EmissionsKg,
    decimal? Co2SavedKg,
    decimal? EstimatedCalories);
