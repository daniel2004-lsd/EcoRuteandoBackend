using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Commands.RateRoute;

public sealed record RateRouteCommand(
    Guid RouteId,
    short RatingValue,
    string? Comment,
    long? UsageId,
    Guid UserId)
    : IRequest;