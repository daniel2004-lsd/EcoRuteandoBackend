using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.AddressHistory;

/// <summary>
/// HU-23 / CU27: Guarda una dirección en el historial del usuario autenticado.
/// </summary>
public sealed record SaveAddressHistoryCommand(
    Guid UserId,
    string SearchText,
    decimal Latitude,
    decimal Longitude,
    string? PlaceId = null,
    string SearchType = "search")
    : IRequest<Guid>;
