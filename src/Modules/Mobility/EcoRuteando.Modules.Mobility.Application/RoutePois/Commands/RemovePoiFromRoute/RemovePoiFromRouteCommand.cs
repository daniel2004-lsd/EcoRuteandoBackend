using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.RemovePoiFromRoute;

public sealed record RemovePoiFromRouteCommand(
    Guid RouteId,
    Guid PoiId)
    : IRequest;