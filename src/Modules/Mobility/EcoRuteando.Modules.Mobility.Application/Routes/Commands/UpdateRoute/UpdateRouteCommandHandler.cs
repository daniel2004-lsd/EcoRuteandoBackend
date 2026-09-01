using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Commands.UpdateRoute;

public sealed class UpdateRouteCommandHandler
    : IRequestHandler<UpdateRouteCommand>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public UpdateRouteCommandHandler(
        IRouteRepository routeRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateRouteCommand request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.Id,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        if (!PgEnumExtensions.TryFromPgName(request.TransportType, out TransportType transportType))
        {
            throw new DomainException(
                $"El tipo de transporte '{request.TransportType}' no es válido.");
        }

        Point? startLocation = null;
        if (request.StartLat.HasValue && request.StartLng.HasValue)
        {
            startLocation = new Point(request.StartLng.Value, request.StartLat.Value);
        }

        Point? endLocation = null;
        if (request.EndLat.HasValue && request.EndLng.HasValue)
        {
            endLocation = new Point(request.EndLng.Value, request.EndLat.Value);
        }

        route.Update(
            request.Name,
            request.Description,
            transportType,
            request.StartName,
            request.DestinationName,
            startLocation,
            endLocation,
            request.EncodedPolyline,
            request.DistanceKm,
            request.EstimatedTimeMin,
            request.Co2SavedKg,
            request.EstimatedCalories,
            request.DifficultyLevel,
            request.MapData,
            request.PhotoUrl,
            request.AvailableDate);

        _routeRepository.Update(route);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
