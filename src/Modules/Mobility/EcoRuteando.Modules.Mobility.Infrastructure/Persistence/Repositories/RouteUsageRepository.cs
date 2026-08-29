using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class RouteUsageRepository : IRouteUsageRepository
{
    private readonly MobilityDbContext _dbContext;

    public RouteUsageRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RouteUsage?> GetByIdAsync(
        Guid usageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RouteUsages
            .AsNoTracking()
            .Include(ru => ru.Route)
            .FirstOrDefaultAsync(
                ru => ru.Id == usageId && ru.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RouteUsage>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RouteUsages
            .AsNoTracking()
            .Include(ru => ru.Route)
            .Where(ru => ru.UserId == userId)
            .OrderByDescending(ru => ru.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        RouteUsage routeUsage,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RouteUsages.AddAsync(routeUsage, cancellationToken);
    }

    public void Update(RouteUsage routeUsage)
    {
        _dbContext.RouteUsages.Update(routeUsage);
    }
}
