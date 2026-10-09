using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using NetTopologySuite.Geometries;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Commands.CreateRoute;

public sealed class CreateRouteCommandHandler
    : IRequestHandler<CreateRouteCommand, Guid>
{
    private readonly IRouteRepository _routeRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public CreateRouteCommandHandler(
        IRouteRepository routeRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateRouteCommand request,
        CancellationToken cancellationToken)
    {
        if (!PgEnumExtensions.TryFromPgName(request.TransportType, out TransportType transportType))
        {
            throw new Shared.Exceptions.DomainException(
                $"El tipo de transporte '{request.TransportType}' no es válido.");
        }

        // Regla de negocio CU03: no permitir rutas duplicadas por nombre.
        if (await _routeRepository.ExistsByNameAsync(
                request.Name,
                cancellationToken: cancellationToken))
        {
            throw new Shared.Exceptions.DomainException(
                $"Ya existe una ruta con el nombre '{request.Name}'.");
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

        var route = new Route(
            request.Name,
            transportType,
            request.StartName,
            request.DestinationName,
            request.Description,
            startLocation,
            endLocation,
            null,
            request.EncodedPolyline,
            request.DistanceKm,
            request.EstimatedTimeMin,
            request.Co2SavedKg,
            request.EstimatedCalories,
            request.DifficultyLevel,
            request.MapData,
            request.PhotoUrl,
            request.AvailableDate,
            request.CreatedBy);

        await _routeRepository.AddAsync(route, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return route.Id;
    }
}
