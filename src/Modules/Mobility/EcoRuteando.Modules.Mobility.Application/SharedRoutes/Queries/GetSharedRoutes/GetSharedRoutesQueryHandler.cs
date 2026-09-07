using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Queries.GetSharedRoutes;

public sealed class GetSharedRoutesQueryHandler
    : IRequestHandler<GetSharedRoutesQuery, IReadOnlyList<GetSharedRoutesResponse>>
{
    private readonly ISharedRouteRepository _sharedRouteRepository;
    private readonly IRouteUsageRepository _routeUsageRepository;

    public GetSharedRoutesQueryHandler(
        ISharedRouteRepository sharedRouteRepository,
        IRouteUsageRepository routeUsageRepository)
    {
        _sharedRouteRepository = sharedRouteRepository;
        _routeUsageRepository = routeUsageRepository;
    }

    public async Task<IReadOnlyList<GetSharedRoutesResponse>> Handle(
        GetSharedRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var sharedRoutes = await _sharedRouteRepository.GetByUserAsync(
            request.UserId,
            cancellationToken);

        var usages = await _routeUsageRepository.GetByUserAsync(
            request.UserId,
            cancellationToken);

        var usagesById = usages
            .Cast<RouteUsage>()
            .GroupBy(ru => ru.Id)
            .ToDictionary(g => g.Key, g => g.First());

        return sharedRoutes
            .Select(sr =>
            {
                var usage = usagesById.TryGetValue(sr.UsageId, out var u) ? u : null;
                return new GetSharedRoutesResponse(
                    sr.Id,
                    sr.UsageId,
                    sr.SocialNetwork,
                    sr.Confirmed,
                    sr.CreatedAt,
                    usage?.ActualDistanceKm,
                    usage?.ActualDurationMin,
                    usage?.ActualCo2Kg,
                    usage?.Route?.Name,
                    usage?.Route?.StartName,
                    usage?.Route?.DestinationName);
            })
            .ToList();
    }
}