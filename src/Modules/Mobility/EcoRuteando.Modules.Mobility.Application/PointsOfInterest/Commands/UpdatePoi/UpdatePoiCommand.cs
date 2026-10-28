using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.UpdatePoi;

public sealed record UpdatePoiCommand(
    Guid Id,
    string Name,
    string PoiType,
    double? Lat = null,
    double? Lng = null,
    string? Description = null,
    string? Address = null,
    string? IconUrl = null,
    Guid? RequestedByUserId = null,
    bool IsAdmin = false)
    : IRequest;