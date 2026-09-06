using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class SharedRouteRepository : ISharedRouteRepository
{
    private readonly MobilityDbContext _dbContext;

    public SharedRouteRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SharedRoute?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SharedRoutes
            .AsNoTracking()
            .Include(sr => sr.Usage)
            .FirstOrDefaultAsync(
                sr => sr.Id == id && sr.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SharedRoute>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SharedRoutes
            .AsNoTracking()
            .Include(sr => sr.Usage)
            .Where(sr => sr.UserId == userId)
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        SharedRoute sharedRoute,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SharedRoutes.AddAsync(sharedRoute, cancellationToken);
    }

    public void Delete(SharedRoute sharedRoute)
    {
        _dbContext.SharedRoutes.Remove(sharedRoute);
    }
}