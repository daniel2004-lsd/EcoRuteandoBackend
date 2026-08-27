using System.Text.Json;
using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

public sealed class PointOfInterest : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string PoiType { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Point Location { get; private set; } = default!;

    public string? Address { get; private set; }

    public string? IconUrl { get; private set; }

    public bool IsActive { get; private set; }

    public string? Source { get; private set; }

    public JsonDocument? ExternalData { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public ICollection<RoutePoi> RoutePois { get; private set; } = [];

    private PointOfInterest()
    {
    }

    public PointOfInterest(
        string name,
        string poiType,
        Point location,
        string? description = null,
        string? address = null,
        string? iconUrl = null,
        string? source = null,
        JsonDocument? externalData = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del punto de interés es obligatorio.");

        if (name.Trim().Length > 150)
            throw new DomainException("El nombre no puede superar 150 caracteres.");

        if (string.IsNullOrWhiteSpace(poiType))
            throw new DomainException("El tipo de punto de interés es obligatorio.");

        if (poiType.Trim().Length > 80)
            throw new DomainException("El tipo no puede superar 80 caracteres.");

        if (location is null)
            throw new DomainException("La ubicación del punto de interés es obligatoria.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        PoiType = poiType.Trim();
        Location = location;
        Description = description?.Trim();
        Address = address?.Trim();
        IconUrl = iconUrl?.Trim();
        IsActive = true;
        Source = source?.Trim();
        ExternalData = externalData;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
