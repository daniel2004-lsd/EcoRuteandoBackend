using System.Text.Json;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.StartTrip;

public sealed class StartTripCommandHandler
    : IRequestHandler<StartTripCommand, Guid>
{
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public StartTripCommandHandler(
        IRouteUsageRepository routeUsageRepository,
        IRouteRepository routeRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeUsageRepository = routeUsageRepository;
        _routeRepository = routeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        StartTripCommand request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
            throw new NotFoundException("La ruta no existe.");

        TransportType? transportMode = null;
        if (!string.IsNullOrWhiteSpace(request.TransportMode)
            && PgEnumExtensions.TryFromPgName(
                request.TransportMode,
                out TransportType parsed))
        {
            transportMode = parsed;
        }

        JsonDocument? gpsData = null;
        if (!string.IsNullOrWhiteSpace(request.GpsDataJson))
        {
            gpsData = JsonDocument.Parse(request.GpsDataJson);
        }

        var routeUsage = new RouteUsage(
            request.UserId,
            request.RouteId,
            transportMode,
            request.Source,
            actualRoute: null,
            gpsData: gpsData);

        await _routeUsageRepository.AddAsync(routeUsage, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return routeUsage.Id;
    }
}
