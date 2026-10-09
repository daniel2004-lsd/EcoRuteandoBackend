using System.Text.Json;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Commands.CreateRoute;

public sealed record CreateRouteCommand(
    string Name,
    string? Description,
    string TransportType,
    string StartName,
    string DestinationName,
    double? StartLat,
    double? StartLng,
    double? EndLat,
    double? EndLng,
    string? EncodedPolyline,
    decimal? DistanceKm,
    int? EstimatedTimeMin,
    decimal? Co2SavedKg,
    decimal? EstimatedCalories,
    short? DifficultyLevel,
    JsonDocument? MapData,
    string? PhotoUrl,
    DateOnly? AvailableDate,
    Guid? CreatedBy) : IRequest<Guid>;
