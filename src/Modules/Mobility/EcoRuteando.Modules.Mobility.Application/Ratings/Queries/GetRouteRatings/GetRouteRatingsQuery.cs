using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetRouteRatings;

public sealed record GetRouteRatingsQuery(
    Guid RouteId)
    : IRequest<GetRouteRatingsResponse>;