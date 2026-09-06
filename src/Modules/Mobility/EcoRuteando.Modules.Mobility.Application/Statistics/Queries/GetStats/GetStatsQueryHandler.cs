using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Statistics.Queries.GetStats;

public sealed class GetStatsQueryHandler
    : IRequestHandler<GetStatsQuery, GetStatsResponse>
{
    private readonly IRouteUsageRepository _routeUsageRepository;

    public GetStatsQueryHandler(
        IRouteUsageRepository routeUsageRepository)
    {
        _routeUsageRepository = routeUsageRepository;
    }

    public async Task<GetStatsResponse> Handle(
        GetStatsQuery request,
        CancellationToken cancellationToken)
    {
        var analytics = await _routeUsageRepository.GetAnalyticsAsync(
            request.From,
            request.To,
            cancellationToken);

        return new GetStatsResponse(
            analytics.RoutesConsulted,
            analytics.ActiveUsers,
            analytics.TotalCo2Kg,
            analytics.CompletedTrips,
            analytics.AverageDistanceKm,
            analytics.ByTransportMode
                .Select(s => new TransportModeStatResponse(
                    s.Mode,
                    s.Trips,
                    s.Co2Kg,
                    s.Users))
                .ToList(),
            analytics.Monthly
                .Select(m => new MonthStatResponse(
                    m.Period,
                    m.Trips,
                    m.Co2Kg))
                .ToList());
    }
}