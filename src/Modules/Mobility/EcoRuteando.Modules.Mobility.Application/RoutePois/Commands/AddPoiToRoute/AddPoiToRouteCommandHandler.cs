using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.AddPoiToRoute;

public sealed class AddPoiToRouteCommandHandler
    : IRequestHandler<AddPoiToRouteCommand>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IPointOfInterestRepository _pointOfInterestRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public AddPoiToRouteCommandHandler(
        IRouteRepository routeRepository,
        IPointOfInterestRepository pointOfInterestRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeRepository = routeRepository;
        _pointOfInterestRepository = pointOfInterestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AddPoiToRouteCommand request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        // Regla de control de acceso y propiedad (H-15)
        if (!request.IsAdmin
            && (route.CreatedBy is null
                || route.CreatedBy != request.RequestedByUserId))
        {
            throw new ForbiddenException(
                "Solo puedes asociar puntos de interés a las rutas que tú mismo creaste.");
        }

        var poi = await _pointOfInterestRepository.GetByIdAsync(
            request.PoiId,
            cancellationToken);

        if (poi is null)
        {
            throw new NotFoundException("El punto de interés no existe.");
        }

        if (!poi.IsActive)
        {
            throw new DomainException("El punto de interés no está activo.");
        }

        // Si el POI es privado de otro usuario, evitar asociación no autorizada
        if (!request.IsAdmin
            && poi.CreatedBy.HasValue
            && poi.CreatedBy != request.RequestedByUserId)
        {
            throw new ForbiddenException(
                "No puedes asociar un punto de interés privado perteneciente a otro usuario.");
        }

        route.AddPoi(request.PoiId, request.SortOrder);

        _routeRepository.Update(route);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}