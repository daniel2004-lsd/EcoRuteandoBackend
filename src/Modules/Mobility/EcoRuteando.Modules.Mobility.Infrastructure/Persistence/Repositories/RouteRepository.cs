using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class RouteRepository : IRouteRepository
{
    private readonly MobilityDbContext _dbContext;

    public RouteRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Route?> GetWithPoisByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Routes
            .Include(r => r.RoutePois
                .OrderBy(rp => rp.SortOrder)
                    .ThenBy(rp => rp.PointOfInterest.Name))
                .ThenInclude(rp => rp.PointOfInterest)
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Route>> GetActiveAsync(
        TransportType? transportType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Routes
            .AsNoTracking()
            .Where(r => r.Status == RouteStatus.Active);

        if (transportType.HasValue)
        {
            query = query.Where(r => r.TransportType == transportType.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Route route,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Routes.AddAsync(route, cancellationToken);
    }

    public void Update(Route route)
    {
        _dbContext.Routes.Update(route);
    }

    public void Delete(Route route)
    {
        _dbContext.Routes.Remove(route);
    }
}
