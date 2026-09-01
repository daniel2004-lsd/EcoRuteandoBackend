using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPoiById;

public sealed class GetPoiByIdQueryHandler
    : IRequestHandler<GetPoiByIdQuery, GetPoiByIdResponse>
{
    private readonly IPointOfInterestRepository _pointOfInterestRepository;

    public GetPoiByIdQueryHandler(
        IPointOfInterestRepository pointOfInterestRepository)
    {
        _pointOfInterestRepository = pointOfInterestRepository;
    }

    public async Task<GetPoiByIdResponse> Handle(
        GetPoiByIdQuery request,
        CancellationToken cancellationToken)
    {
        var poi = await _pointOfInterestRepository.GetByIdAsync(
            request.PoiId,
            cancellationToken);

        if (poi is null)
        {
            throw new NotFoundException("El punto de interés no existe.");
        }

        return new GetPoiByIdResponse(
            poi.Id,
            poi.Name,
            poi.PoiType,
            poi.Description,
            poi.Location.Y,
            poi.Location.X,
            poi.Address,
            poi.IconUrl,
            poi.IsActive,
            poi.Source,
            poi.CreatedAt,
            poi.UpdatedAt);
    }
}