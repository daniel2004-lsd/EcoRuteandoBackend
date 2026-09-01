using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRouteById;

public sealed record GetRouteByIdQuery(
    Guid RouteId)
    : IRequest<GetRouteByIdResponse>;
