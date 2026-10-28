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

    public async Task<IReadOnlyList<Route>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Routes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Route>> GetRoutesAsync(
        TransportType? transportType = null,
        bool includeInactive = false,
        Guid? createdBy = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Routes.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(r => r.Status == RouteStatus.Active);
        }

        if (transportType.HasValue)
        {
            query = query.Where(r => r.TransportType == transportType.Value);
        }

        if (createdBy.HasValue)
        {
            query = query.Where(r => r.CreatedBy == createdBy.Value);
        }

        var clampedPage = Math.Max(1, page);
        var clampedPageSize = Math.Clamp(pageSize, 1, 100);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((clampedPage - 1) * clampedPageSize)
            .Take(clampedPageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return await _dbContext.Routes
            .AnyAsync(
                r => r.Name.Trim().ToLower() == normalized
                     && (!excludeId.HasValue || r.Id != excludeId.Value),
                cancellationToken);
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
