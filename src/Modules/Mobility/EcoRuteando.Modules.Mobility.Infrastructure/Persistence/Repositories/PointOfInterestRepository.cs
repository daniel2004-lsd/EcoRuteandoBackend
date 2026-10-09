using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class PointOfInterestRepository : IPointOfInterestRepository
{
    private readonly MobilityDbContext _dbContext;

    public PointOfInterestRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PointOfInterest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PointsOfInterest
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PointOfInterest>> GetActiveAsync(
        string? poiType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PointsOfInterest
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(poiType))
        {
            query = query.Where(p => p.PoiType == poiType.Trim());
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        PointOfInterest pointOfInterest,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PointsOfInterest.AddAsync(
            pointOfInterest,
            cancellationToken);
    }

    public void Update(PointOfInterest pointOfInterest)
    {
        _dbContext.PointsOfInterest.Update(pointOfInterest);
    }
}
