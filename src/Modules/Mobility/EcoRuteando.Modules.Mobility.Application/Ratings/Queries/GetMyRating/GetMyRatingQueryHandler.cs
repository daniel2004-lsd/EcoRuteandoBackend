using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetMyRating;

public sealed class GetMyRatingQueryHandler
    : IRequestHandler<GetMyRatingQuery, GetMyRatingResponse?>
{
    private readonly IRatingRepository _ratingRepository;

    public GetMyRatingQueryHandler(
        IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<GetMyRatingResponse?> Handle(
        GetMyRatingQuery request,
        CancellationToken cancellationToken)
    {
        var rating = await _ratingRepository.GetByUserAndRouteAsync(
            request.UserId,
            request.RouteId,
            cancellationToken);

        if (rating is null)
        {
            return null;
        }

        return new GetMyRatingResponse(
            rating.Id,
            rating.RouteId,
            rating.RatingValue,
            rating.Comment,
            rating.CreatedAt,
            rating.UpdatedAt);
    }
}