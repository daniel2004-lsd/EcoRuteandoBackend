using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.CreatePoi;

public sealed class CreatePoiCommandHandler
    : IRequestHandler<CreatePoiCommand, Guid>
{
    private readonly IPointOfInterestRepository _pointOfInterestRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public CreatePoiCommandHandler(
        IPointOfInterestRepository pointOfInterestRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _pointOfInterestRepository = pointOfInterestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreatePoiCommand request,
        CancellationToken cancellationToken)
    {
        // En NetTopologySuite: X = longitud, Y = latitud
        var location = new Point(request.Lng, request.Lat);

        var poi = new PointOfInterest(
            request.Name,
            request.PoiType,
            location,
            request.Description,
            request.Address,
            request.IconUrl,
            request.Source,
            createdBy: request.CreatedBy);

        await _pointOfInterestRepository.AddAsync(poi, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return poi.Id;
    }
}