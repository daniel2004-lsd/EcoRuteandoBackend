using System.Text.Json;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

public sealed class Route : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TransportType TransportType { get; private set; }

    public RouteStatus Status { get; private set; }

    public string StartName { get; private set; } = string.Empty;

    public string DestinationName { get; private set; } = string.Empty;

    public Point? StartLocation { get; private set; }

    public Point? EndLocation { get; private set; }

    public LineString? RouteGeometry { get; private set; }

    public string? EncodedPolyline { get; private set; }

    public decimal? DistanceKm { get; private set; }

    public int? EstimatedTimeMin { get; private set; }

    public decimal? Co2SavedKg { get; private set; }

    public decimal? EstimatedCalories { get; private set; }

    public short? DifficultyLevel { get; private set; }

    public JsonDocument? MapData { get; private set; }

    public string? PhotoUrl { get; private set; }

    public DateOnly? AvailableDate { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public ICollection<RoutePoi> RoutePois { get; private set; } = [];

    private Route()
    {
    }

    public Route(
        string name,
        TransportType transportType,
        string startName,
        string destinationName,
        string? description = null,
        Point? startLocation = null,
        Point? endLocation = null,
        LineString? routeGeometry = null,
        string? encodedPolyline = null,
        decimal? distanceKm = null,
        int? estimatedTimeMin = null,
        decimal? co2SavedKg = null,
        decimal? estimatedCalories = null,
        short? difficultyLevel = null,
        JsonDocument? mapData = null,
        string? photoUrl = null,
        DateOnly? availableDate = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la ruta es obligatorio.");

        if (name.Trim().Length > 150)
            throw new DomainException("El nombre de la ruta no puede superar 150 caracteres.");

        if (string.IsNullOrWhiteSpace(startName))
            throw new DomainException("El punto de origen es obligatorio.");

        if (string.IsNullOrWhiteSpace(destinationName))
            throw new DomainException("El punto de destino es obligatorio.");

        if (difficultyLevel is < 1 or > 5)
            throw new DomainException("El nivel de dificultad debe estar entre 1 y 5.");

        if (distanceKm is < 0)
            throw new DomainException("La distancia no puede ser negativa.");

        if (estimatedTimeMin is < 0)
            throw new DomainException("El tiempo estimado no puede ser negativo.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim();
        TransportType = transportType;
        Status = RouteStatus.Active;
        StartName = startName.Trim();
        DestinationName = destinationName.Trim();
        StartLocation = startLocation;
        EndLocation = endLocation;
        RouteGeometry = routeGeometry;
        EncodedPolyline = encodedPolyline?.Trim();
        DistanceKm = distanceKm;
        EstimatedTimeMin = estimatedTimeMin;
        Co2SavedKg = co2SavedKg;
        EstimatedCalories = estimatedCalories;
        DifficultyLevel = difficultyLevel;
        MapData = mapData;
        PhotoUrl = photoUrl?.Trim();
        AvailableDate = availableDate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? description,
        TransportType transportType,
        string startName,
        string destinationName,
        Point? startLocation,
        Point? endLocation,
        string? encodedPolyline,
        decimal? distanceKm,
        int? estimatedTimeMin,
        decimal? co2SavedKg,
        decimal? estimatedCalories,
        short? difficultyLevel,
        JsonDocument? mapData,
        string? photoUrl,
        DateOnly? availableDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la ruta es obligatorio.");

        if (name.Trim().Length > 150)
            throw new DomainException("El nombre de la ruta no puede superar 150 caracteres.");

        if (string.IsNullOrWhiteSpace(startName))
            throw new DomainException("El punto de origen es obligatorio.");

        if (string.IsNullOrWhiteSpace(destinationName))
            throw new DomainException("El punto de destino es obligatorio.");

        if (difficultyLevel is < 1 or > 5)
            throw new DomainException("El nivel de dificultad debe estar entre 1 y 5.");

        if (distanceKm is < 0)
            throw new DomainException("La distancia no puede ser negativa.");

        if (estimatedTimeMin is < 0)
            throw new DomainException("El tiempo estimado no puede ser negativo.");

        Name = name.Trim();
        Description = description?.Trim();
        TransportType = transportType;
        StartName = startName.Trim();
        DestinationName = destinationName.Trim();
        StartLocation = startLocation;
        EndLocation = endLocation;
        EncodedPolyline = encodedPolyline?.Trim();
        DistanceKm = distanceKm;
        EstimatedTimeMin = estimatedTimeMin;
        Co2SavedKg = co2SavedKg;
        EstimatedCalories = estimatedCalories;
        DifficultyLevel = difficultyLevel;
        MapData = mapData;
        PhotoUrl = photoUrl?.Trim();
        AvailableDate = availableDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Asocia un punto de interés a la ruta con su orden de visita.
    /// </summary>
    public void AddPoi(Guid poiId, short? sortOrder = null)
    {
        if (poiId == Guid.Empty)
            throw new DomainException("El punto de interés es obligatorio.");

        if (RoutePois.Any(rp => rp.PoiId == poiId))
            throw new DomainException("El punto de interés ya está asociado a la ruta.");

        RoutePois.Add(new RoutePoi(Id, poiId, sortOrder));
        UpdatedAt = DateTime.UtcNow;
    }
}
