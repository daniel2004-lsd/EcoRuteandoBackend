using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.DeactivatePoi;

public sealed class DeactivatePoiCommandHandler
    : IRequestHandler<DeactivatePoiCommand>
{
    private readonly IPointOfInterestRepository _pointOfInterestRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public DeactivatePoiCommandHandler(
        IPointOfInterestRepository pointOfInterestRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _pointOfInterestRepository = pointOfInterestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeactivatePoiCommand request,
        CancellationToken cancellationToken)
    {
        var poi = await _pointOfInterestRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (poi is null)
        {
            throw new NotFoundException("El punto de interés no existe.");
        }

        poi.Deactivate();

        _pointOfInterestRepository.Update(poi);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}