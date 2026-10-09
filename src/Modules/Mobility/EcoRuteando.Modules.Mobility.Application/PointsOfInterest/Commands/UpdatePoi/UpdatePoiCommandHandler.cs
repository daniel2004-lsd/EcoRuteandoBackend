using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.UpdatePoi;

public sealed class UpdatePoiCommandHandler
    : IRequestHandler<UpdatePoiCommand>
{
    private readonly IPointOfInterestRepository _pointOfInterestRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public UpdatePoiCommandHandler(
        IPointOfInterestRepository pointOfInterestRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _pointOfInterestRepository = pointOfInterestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdatePoiCommand request,
        CancellationToken cancellationToken)
    {
        var poi = await _pointOfInterestRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (poi is null)
        {
            throw new NotFoundException("El punto de interés no existe.");
        }

        Point? location = null;
        if (request.Lat.HasValue && request.Lng.HasValue)
        {
            // En NetTopologySuite: X = longitud, Y = latitud
            location = new Point(request.Lng.Value, request.Lat.Value);
        }

        poi.Update(
            request.Name,
            request.PoiType,
            request.Description,
            request.Address,
            request.IconUrl,
            location);

        _pointOfInterestRepository.Update(poi);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}