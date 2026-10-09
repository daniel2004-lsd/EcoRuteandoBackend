using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class RatingRepository : IRatingRepository
{
    private readonly MobilityDbContext _dbContext;

    public RatingRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Rating?> GetByUserAndRouteAsync(
        Guid userId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Ratings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.UserId == userId && r.RouteId == routeId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Rating>> GetByRouteAsync(
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Ratings
            .AsNoTracking()
            .Where(r => r.RouteId == routeId && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Rating rating,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Ratings.AddAsync(rating, cancellationToken);
    }

    public void Update(Rating rating)
    {
        _dbContext.Ratings.Update(rating);
    }
}