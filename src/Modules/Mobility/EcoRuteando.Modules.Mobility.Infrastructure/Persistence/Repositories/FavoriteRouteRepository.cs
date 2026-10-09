using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class FavoriteRouteRepository : IFavoriteRouteRepository
{
    private readonly MobilityDbContext _dbContext;

    public FavoriteRouteRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FavoriteRoute?> GetAsync(
        Guid userId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FavoriteRoutes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                fr => fr.UserId == userId && fr.RouteId == routeId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<FavoriteRoute>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FavoriteRoutes
            .AsNoTracking()
            .Include(fr => fr.Route)
            .Where(fr => fr.UserId == userId)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        FavoriteRoute favoriteRoute,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.FavoriteRoutes.AddAsync(favoriteRoute, cancellationToken);
    }

    public void Delete(FavoriteRoute favoriteRoute)
    {
        _dbContext.FavoriteRoutes.Remove(favoriteRoute);
    }
}
