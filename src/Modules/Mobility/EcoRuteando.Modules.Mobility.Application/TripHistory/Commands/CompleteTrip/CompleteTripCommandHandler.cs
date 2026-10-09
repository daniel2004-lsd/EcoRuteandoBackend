using System.Text.Json;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.CompleteTrip;

public sealed class CompleteTripCommandHandler
    : IRequestHandler<CompleteTripCommand>
{
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public CompleteTripCommandHandler(
        IRouteUsageRepository routeUsageRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeUsageRepository = routeUsageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteTripCommand request,
        CancellationToken cancellationToken)
    {
        var routeUsage = await _routeUsageRepository.GetByIdAsync(
            request.UsageId,
            request.UserId,
            cancellationToken);

        if (routeUsage is null)
            throw new NotFoundException("El trayecto no existe.");

        JsonDocument? gpsData = null;
        if (!string.IsNullOrWhiteSpace(request.GpsDataJson))
        {
            gpsData = JsonDocument.Parse(request.GpsDataJson);
        }

        routeUsage.Complete(
            request.ActualDistanceKm,
            request.ActualDurationMin,
            request.ActualCo2Kg,
            request.EndedAt,
            actualRoute: null,
            gpsData: gpsData);

        _routeUsageRepository.Update(routeUsage);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
