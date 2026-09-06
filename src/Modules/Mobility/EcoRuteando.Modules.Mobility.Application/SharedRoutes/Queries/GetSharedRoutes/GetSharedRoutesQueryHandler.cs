using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.SharedRoutes.Queries.GetSharedRoutes;

public sealed class GetSharedRoutesQueryHandler
    : IRequestHandler<GetSharedRoutesQuery, IReadOnlyList<GetSharedRoutesResponse>>
{
    private readonly ISharedRouteRepository _sharedRouteRepository;

    public GetSharedRoutesQueryHandler(
        ISharedRouteRepository sharedRouteRepository)
    {
        _sharedRouteRepository = sharedRouteRepository;
    }

    public async Task<IReadOnlyList<GetSharedRoutesResponse>> Handle(
        GetSharedRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var sharedRoutes = await _sharedRouteRepository.GetByUserAsync(
            request.UserId,
            cancellationToken);

        return sharedRoutes
            .Select(sr => new GetSharedRoutesResponse(
                sr.Id,
                sr.UsageId,
                sr.SocialNetwork,
                sr.Confirmed,
                sr.CreatedAt,
                sr.Usage?.ActualDistanceKm,
                sr.Usage?.ActualDurationMin,
                sr.Usage?.ActualCo2Kg,
                sr.Usage?.Route?.Name,
                sr.Usage?.Route?.StartName,
                sr.Usage?.Route?.DestinationName))
            .ToList();
    }
}