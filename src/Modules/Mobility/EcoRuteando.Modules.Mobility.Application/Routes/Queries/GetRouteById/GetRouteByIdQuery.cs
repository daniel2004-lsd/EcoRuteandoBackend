using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRouteById;

public sealed record GetRouteByIdQuery(
    Guid RouteId,
    Guid? RequestedByUserId = null,
    bool IsAdmin = false)
    : IRequest<GetRouteByIdResponse>;
