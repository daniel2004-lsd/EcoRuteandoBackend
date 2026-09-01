using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripHistory;

public sealed class GetTripHistoryQueryHandler
    : IRequestHandler<GetTripHistoryQuery, IReadOnlyList<GetTripHistoryResponse>>
{
    private readonly IRouteUsageRepository _routeUsageRepository;

    public GetTripHistoryQueryHandler(
        IRouteUsageRepository routeUsageRepository)
    {
        _routeUsageRepository = routeUsageRepository;
    }

    public async Task<IReadOnlyList<GetTripHistoryResponse>> Handle(
        GetTripHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var usages = await _routeUsageRepository.GetByUserAsync(
            request.UserId,
            cancellationToken);

        return usages
            .Select(u => new GetTripHistoryResponse(
                u.Id,
                u.RouteId,
                u.Route?.Name ?? "Ruta eliminada",
                u.TransportMode?.ToPgName(),
                u.Source.ToPgName(),
                u.StartedAt,
                u.EndedAt,
                u.Completed,
                u.ActualDistanceKm,
                u.ActualDurationMin,
                u.ActualCo2Kg,
                u.Route?.EstimatedCalories))
            .ToList();
    }
}
