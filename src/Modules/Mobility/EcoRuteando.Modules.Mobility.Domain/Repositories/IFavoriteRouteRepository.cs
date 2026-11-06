using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IFavoriteRouteRepository
{
    Task<FavoriteRoute?> GetAsync(
        Guid userId,
        Guid routeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FavoriteRoute>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FavoriteRoute favoriteRoute,
        CancellationToken cancellationToken = default);

    void Delete(FavoriteRoute favoriteRoute);

    /// <summary>
    /// Cuenta el número total de rutas favoritas marcadas por el usuario.
    /// </summary>
    Task<int> CountByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
