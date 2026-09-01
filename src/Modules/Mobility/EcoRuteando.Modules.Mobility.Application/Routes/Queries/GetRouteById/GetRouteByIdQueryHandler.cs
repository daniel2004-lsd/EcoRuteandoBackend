using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRouteById;

public sealed class GetRouteByIdQueryHandler
    : IRequestHandler<GetRouteByIdQuery, GetRouteByIdResponse>
{
    private readonly IRouteRepository _routeRepository;

    public GetRouteByIdQueryHandler(
        IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<GetRouteByIdResponse> Handle(
        GetRouteByIdQuery request,
        CancellationToken cancellationToken)
    {
        var route = await _routeRepository.GetWithPoisByIdAsync(
            request.RouteId,
            cancellationToken);

        if (route is null)
        {
            throw new NotFoundException("La ruta no existe.");
        }

        return new GetRouteByIdResponse(
            route.Id,
            route.Name,
            route.Description,
            route.TransportType.ToPgName(),
            route.Status.ToPgName(),
            route.StartName,
            route.DestinationName,
            ToGeoPoint(route.StartLocation),
            ToGeoPoint(route.EndLocation),
            route.EncodedPolyline,
            route.DistanceKm,
            route.EstimatedTimeMin,
            route.Co2SavedKg,
            route.EstimatedCalories,
            route.DifficultyLevel,
            route.MapData,
            route.PhotoUrl,
            route.AvailableDate,
            route.RoutePois
                .Select(rp => new RoutePoiResponse(
                    rp.PointOfInterest.Id,
                    rp.PointOfInterest.Name,
                    rp.PointOfInterest.PoiType,
                    rp.PointOfInterest.Description,
                    rp.PointOfInterest.Location.Y,
                    rp.PointOfInterest.Location.X,
                    rp.PointOfInterest.Address,
                    rp.PointOfInterest.IconUrl))
                .ToList());
    }

    // En NetTopologySuite: X = longitud, Y = latitud
    private static GeoPointResponse? ToGeoPoint(
        NetTopologySuite.Geometries.Point? point)
    {
        return point is null
            ? null
            : new GeoPointResponse(point.Y, point.X);
    }
}
