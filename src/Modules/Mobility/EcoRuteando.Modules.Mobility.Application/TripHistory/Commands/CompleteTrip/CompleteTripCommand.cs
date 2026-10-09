using System.Text.Json;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.CompleteTrip;

public sealed record CompleteTripCommand(
    Guid UsageId,
    Guid UserId,
    decimal? ActualDistanceKm,
    int? ActualDurationMin,
    decimal? ActualCo2Kg,
    DateTime? EndedAt,
    string? GpsDataJson)
    : IRequest;
