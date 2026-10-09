using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetMyRating;

public sealed record GetMyRatingQuery(
    Guid RouteId,
    Guid UserId)
    : IRequest<GetMyRatingResponse?>;