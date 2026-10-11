namespace EcoRuteando.Modules.Mobility.Application.AddressHistory;

/// <summary>
/// Respuesta del historial de direcciones del usuario (HU-23).
/// </summary>
public sealed record GetUserAddressHistoryResponse
{
    public Guid Id { get; init; }

    public string? SearchText { get; init; }

    public Guid? PlaceId { get; init; }

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public string? SearchType { get; init; }

    public DateTime CreatedAt { get; init; }
}
