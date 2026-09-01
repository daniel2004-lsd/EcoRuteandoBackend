using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRoutes;

public sealed class GetRoutesQueryHandler
    : IRequestHandler<GetRoutesQuery, IReadOnlyList<GetRoutesResponse>>
{
    private readonly IRouteRepository _routeRepository;

    public GetRoutesQueryHandler(
        IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<IReadOnlyList<GetRoutesResponse>> Handle(
        GetRoutesQuery request,
        CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetActiveAsync(
            request.TransportType,
            cancellationToken);

        return routes
            .Select(r => new GetRoutesResponse(
                r.Id,
                r.Name,
                r.TransportType.ToPgName(),
                r.Status.ToPgName(),
                r.StartName,
                r.DestinationName,
                r.DistanceKm,
                r.EstimatedTimeMin,
                r.Co2SavedKg,
                r.DifficultyLevel,
                r.PhotoUrl))
            .ToList();
    }
}
