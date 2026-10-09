using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.AddressHistory;

/// <summary>
/// HU-23: Obtiene el historial de direcciones guardadas del usuario autenticado.
/// Permite reutilizar direcciones anteriores al planificar nuevas rutas.
/// </summary>
public sealed record GetUserAddressHistoryQuery(Guid UserId)
    : IRequest<IReadOnlyList<GetUserAddressHistoryResponse>>;