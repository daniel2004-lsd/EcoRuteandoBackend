using System.Text.Json;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.StartTrip;

public sealed record StartTripCommand(
    Guid RouteId,
    string? TransportMode,
    string Source,
    Guid UserId,
    string? GpsDataJson)
    : IRequest<Guid>;
