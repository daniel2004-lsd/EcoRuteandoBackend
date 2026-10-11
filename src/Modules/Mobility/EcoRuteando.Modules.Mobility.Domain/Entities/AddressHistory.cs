using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Entrada del historial de direcciones del usuario (HU-23).
/// Guarda búsquedas de ubicaciones para reutilizarlas al planificar rutas.
/// </summary>
public sealed class AddressHistory : Entity<Guid>
{
    public Guid UserId { get; private set; }

    public string SearchText { get; private set; } = string.Empty;

    public Guid? PlaceId { get; private set; }

    public Point Location { get; private set; } = default!;

    public string SearchType { get; private set; } = string.Empty;

    private AddressHistory()
    {
    }

    public AddressHistory(
        Guid userId,
        string searchText,
        Guid? placeId,
        Point location,
        string searchType)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(searchText))
            throw new DomainException("El texto de búsqueda es obligatorio.");

        if (searchText.Trim().Length > 200)
            throw new DomainException("El texto de búsqueda no puede superar 200 caracteres.");

        if (location is null)
            throw new DomainException("La ubicación es obligatoria.");

        if (string.IsNullOrWhiteSpace(searchType))
            throw new DomainException("El tipo de búsqueda es obligatorio.");

        Id = Guid.NewGuid();
        UserId = userId;
        SearchText = searchText.Trim();
        PlaceId = placeId;
        Location = location;
        SearchType = searchType.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
