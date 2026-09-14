using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.InterruptTrip;

/// <summary>
/// Registra la interrupción de un trayecto con las métricas parciales reales.
/// El usuario no llegó al destino, pero se cuenta el CO₂ ahorrado hasta donde sí viajó.
/// </summary>
public sealed record InterruptTripCommand(
    Guid UsageId,
    Guid UserId,
    decimal? ActualDistanceKm,
    int? ActualDurationMin,
    decimal? ActualCo2Kg,
    DateTime? EndedAt,
    string? GpsDataJson)
    : IRequest;
