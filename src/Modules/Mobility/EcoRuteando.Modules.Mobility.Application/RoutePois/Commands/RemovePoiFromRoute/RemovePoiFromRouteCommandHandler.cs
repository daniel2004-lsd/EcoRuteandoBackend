using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.RemovePoiFromRoute;

public sealed class RemovePoiFromRouteCommandHandler
    : IRequestHandler<RemovePoiFromRouteCommand>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public RemovePoiFromRouteCommandHandler(
        IRouteRepository routeRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemovePoiFromRouteCommand request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        var routePoi = route.RoutePois.FirstOrDefault(
            rp => rp.PoiId == request.PoiId);

        if (routePoi is null)
        {
            throw new NotFoundException(
                "El punto de interés no está asociado a la ruta.");
        }

        route.RoutePois.Remove(routePoi);

        _routeRepository.Update(route);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}