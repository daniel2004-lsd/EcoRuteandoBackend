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
        var routes = request.IncludeInactive
            ? await _routeRepository.GetAllAsync(cancellationToken)
            : await _routeRepository.GetActiveAsync(
                request.TransportType,
                cancellationToken);

        // Filtrar por usuario si se solicita (mine=true)
        if (request.CreatedBy.HasValue)
        {
            routes = routes
                .Where(r => r.CreatedBy == request.CreatedBy.Value)
                .ToList();
        }

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
                r.PhotoUrl,
                r.EncodedPolyline,
                r.StartLocation?.Y,
                r.StartLocation?.X,
                r.EndLocation?.Y,
                r.EndLocation?.X,
                r.EstimatedCalories))
            .ToList();
    }
}
