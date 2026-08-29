using System.Text.Json;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Registro de uso de una ruta por un usuario (tabla mobility.route_usage).
/// Representa un trayecto iniciado/finalizado, con métricas reales
/// de distancia, duración y CO₂ ahorrado.
/// </summary>
public sealed class RouteUsage : Entity<Guid>
{
    public Guid UserId { get; private set; }

    public Guid RouteId { get; private set; }

    public TransportType? TransportMode { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public bool Completed { get; private set; }

    public decimal? ActualDistanceKm { get; private set; }

    public int? ActualDurationMin { get; private set; }

    public decimal? ActualCo2Kg { get; private set; }

    public LineString? ActualRoute { get; private set; }

    public JsonDocument? GpsData { get; private set; }

    public UsageSource Source { get; private set; }

    public Route? Route { get; private set; }

    private RouteUsage()
    {
    }

    public RouteUsage(
        Guid userId,
        Guid routeId,
        TransportType? transportMode,
        string source,
        DateTime? startedAt = null,
        LineString? actualRoute = null,
        JsonDocument? gpsData = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (routeId == Guid.Empty)
            throw new DomainException("La ruta es obligatoria.");

        if (!PgEnumExtensions.TryFromPgName(source, out UsageSource usageSource))
            throw new DomainException($"El origen '{source}' no es válido.");

        Id = Guid.NewGuid();
        UserId = userId;
        RouteId = routeId;
        TransportMode = transportMode;
        StartedAt = startedAt ?? DateTime.UtcNow;
        Completed = false;
        Source = usageSource;
        ActualRoute = actualRoute;
        GpsData = gpsData;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cierra el trayecto registrando las métricas reales del recorrido.
    /// </summary>
    public void Complete(
        decimal? actualDistanceKm,
        int? actualDurationMin,
        decimal? actualCo2Kg,
        DateTime? endedAt,
        LineString? actualRoute = null,
        JsonDocument? gpsData = null)
    {
        if (EndedAt is not null)
            throw new DomainException("El trayecto ya fue finalizado.");

        if (actualDistanceKm < 0)
            throw new DomainException("La distancia real no puede ser negativa.");

        if (actualDurationMin < 0)
            throw new DomainException("La duración real no puede ser negativa.");

        ActualDistanceKm = actualDistanceKm;
        ActualDurationMin = actualDurationMin;
        ActualCo2Kg = actualCo2Kg;
        EndedAt = endedAt ?? DateTime.UtcNow;

        if (EndedAt < StartedAt)
            throw new DomainException("La fecha de fin no puede ser anterior al inicio.");

        if (actualRoute is not null)
            ActualRoute = actualRoute;

        if (gpsData is not null)
            GpsData = gpsData;

        Completed = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
