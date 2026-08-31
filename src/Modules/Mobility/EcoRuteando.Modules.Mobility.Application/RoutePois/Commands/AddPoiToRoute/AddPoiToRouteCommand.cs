using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.AddPoiToRoute;

public sealed record AddPoiToRouteCommand(
    Guid RouteId,
    Guid PoiId,
    short? SortOrder = null)
    : IRequest;