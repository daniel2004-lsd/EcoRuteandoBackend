using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetRouteRatings;

public sealed class GetRouteRatingsQueryHandler
    : IRequestHandler<GetRouteRatingsQuery, GetRouteRatingsResponse>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IRouteRepository _routeRepository;

    public GetRouteRatingsQueryHandler(
        IRatingRepository ratingRepository,
        IRouteRepository routeRepository)
    {
        _ratingRepository = ratingRepository;
        _routeRepository = routeRepository;
    }

    public async Task<GetRouteRatingsResponse> Handle(
        GetRouteRatingsQuery request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        var ratings = await _ratingRepository.GetByRouteAsync(
            request.RouteId,
            cancellationToken);

        var items = ratings
            .Select(r => new RouteRatingResponse(
                r.Id,
                r.UserId,
                r.RatingValue,
                r.Comment,
                r.CreatedAt))
            .ToList();

        decimal? average = items.Count > 0
            ? (decimal)items.Average(i => i.RatingValue)
            : null;

        return new GetRouteRatingsResponse(
            request.RouteId,
            items,
            average.HasValue ? Math.Round(average.Value, 1) : null,
            items.Count);
    }
}