using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPois;

public sealed class GetPoisQueryHandler
    : IRequestHandler<GetPoisQuery, IReadOnlyList<GetPoisResponse>>
{
    private readonly IPointOfInterestRepository _pointOfInterestRepository;

    public GetPoisQueryHandler(
        IPointOfInterestRepository pointOfInterestRepository)
    {
        _pointOfInterestRepository = pointOfInterestRepository;
    }

    public async Task<IReadOnlyList<GetPoisResponse>> Handle(
        GetPoisQuery request,
        CancellationToken cancellationToken)
    {
        var pois = await _pointOfInterestRepository.GetActiveAsync(
            request.PoiType,
            cancellationToken);

        return pois
            .Select(p => new GetPoisResponse(
                p.Id,
                p.Name,
                p.PoiType,
                p.Description,
                p.Location.Y,
                p.Location.X,
                p.Address,
                p.IconUrl))
            .ToList();
    }
}