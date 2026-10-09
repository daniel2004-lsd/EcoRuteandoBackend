namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Agregación por modo de transporte de los trayectos completados
/// (Portal de estadísticas, CU08).
/// </summary>
public sealed record TransportModeStat(
    string? Mode,
    int Trips,
    decimal Co2Kg,
    int Users);

/// <summary>
/// Agregación mensual de los trayectos completados (CU08).
/// </summary>
public sealed record MonthStat(
    string Period,
    int Trips,
    decimal Co2Kg);

/// <summary>
/// Resumen agregado de trayectos completados dentro de un rango de fechas.
/// Se usa en el portal de estadísticas (CU08): rutas consultadas, usuarios
/// activos, CO₂ ahorrado y desgloses mensuales / por modo de transporte.
/// </summary>
public sealed record RouteUsageAnalytics(
    int CompletedTrips,
    int ActiveUsers,
    int RoutesConsulted,
    decimal TotalCo2Kg,
    double AverageDistanceKm,
    IReadOnlyList<TransportModeStat> ByTransportMode,
    IReadOnlyList<MonthStat> Monthly);