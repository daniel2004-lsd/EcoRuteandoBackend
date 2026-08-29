using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripById;

public sealed class GetTripByIdQueryHandler
    : IRequestHandler<GetTripByIdQuery, GetTripByIdResponse>
{
    private readonly IRouteUsageRepository _routeUsageRepository;

    public GetTripByIdQueryHandler(
        IRouteUsageRepository routeUsageRepository)
    {
        _routeUsageRepository = routeUsageRepository;
    }

    public async Task<GetTripByIdResponse> Handle(
        GetTripByIdQuery request,
        CancellationToken cancellationToken)
    {
        var usage = await _routeUsageRepository.GetByIdAsync(
            request.UsageId,
            request.UserId,
            cancellationToken);

        if (usage is null)
            throw new NotFoundException("El trayecto no existe.");

        return new GetTripByIdResponse(
            usage.Id,
            usage.RouteId,
            usage.Route?.Name ?? "Ruta eliminada",
            usage.TransportMode?.ToPgName(),
            usage.Source.ToPgName(),
            usage.StartedAt,
            usage.EndedAt,
            usage.Completed,
            usage.ActualDistanceKm,
            usage.ActualDurationMin,
            usage.ActualCo2Kg,
            GetTripByIdResponse.ToRoutePoints(usage.ActualRoute),
            usage.GpsData);
    }
}
