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

        // Regla de negocio CU03: un usuario solo puede modificar rutas que él creó.
        // El administrador puede modificar cualquier ruta.
        if (!request.IsAdmin
            && (route.CreatedBy is null
                || route.CreatedBy != request.RequestedByUserId))
        {
            throw new ForbiddenException(
                "Solo puedes modificar las rutas que tú mismo creaste.");
        }

        if (!PgEnumExtensions.TryFromPgName(request.TransportType, out TransportType transportType))
        {
            throw new DomainException(
                $"El tipo de transporte '{request.TransportType}' no es válido.");
        }

        // Regla de negocio CU03: no permitir rutas duplicadas por nombre
        // (se excluye la propia ruta al actualizarla).
        if (await _routeRepository.ExistsByNameAsync(
                request.Name,
                excludeId: request.Id,
                cancellationToken: cancellationToken))
        {
            throw new DomainException(
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
