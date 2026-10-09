using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Queries.GetFavorites;

public sealed class GetFavoritesQueryHandler
    : IRequestHandler<GetFavoritesQuery, IReadOnlyList<GetFavoritesResponse>>
{
    private readonly IFavoriteRouteRepository _favoriteRouteRepository;

    public GetFavoritesQueryHandler(
        IFavoriteRouteRepository favoriteRouteRepository)
    {
        _favoriteRouteRepository = favoriteRouteRepository;
    }

    public async Task<IReadOnlyList<GetFavoritesResponse>> Handle(
        GetFavoritesQuery request,
        CancellationToken cancellationToken)
    {
        var favorites = await _favoriteRouteRepository.GetByUserAsync(
            request.UserId,
            cancellationToken);

        return favorites
            .Select(f => new GetFavoritesResponse(
                f.RouteId,
                f.Route?.Name ?? "Ruta eliminada",
                f.Route?.Description,
                f.Route?.TransportType.ToPgName() ?? string.Empty,
                f.Label,
                f.CreatedAt,
                f.Route?.DistanceKm,
                f.Route?.EstimatedTimeMin,
                f.Route?.Co2SavedKg,
                f.Route?.DifficultyLevel,
                f.Route?.StartLocation != null ? (decimal)f.Route.StartLocation.Y : null,
                f.Route?.StartLocation != null ? (decimal)f.Route.StartLocation.X : null,
                f.Route?.EndLocation != null ? (decimal)f.Route.EndLocation.Y : null,
                f.Route?.EndLocation != null ? (decimal)f.Route.EndLocation.X : null))
            .ToList();
    }
}
