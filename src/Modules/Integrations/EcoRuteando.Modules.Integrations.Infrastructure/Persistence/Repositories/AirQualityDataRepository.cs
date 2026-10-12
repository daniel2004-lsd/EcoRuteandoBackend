using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Repositories;

public class AirQualityDataRepository : IAirQualityDataRepository
{
    private readonly IntegrationsDbContext _dbContext;

    public AirQualityDataRepository(IntegrationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AirQualityData?> GetLatestByCoordinatesAsync(double latitude, double longitude, double maxDistanceMeters = 50000, CancellationToken cancellationToken = default)
    {
        var target = new Point(longitude, latitude) { SRID = 4326 };

        return await _dbContext.AirQualityData
            .AsNoTracking()
            .Where(a => a.Location.IsWithinDistance(target, maxDistanceMeters))
            .OrderByDescending(a => a.MeasuredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AirQualityData>> GetRecentByCityAsync(string city, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AirQualityData
            .AsNoTracking()
            .Where(a => a.City != null && EF.Functions.ILike(a.City, $"%{city}%"))
            .OrderByDescending(a => a.MeasuredAt)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AirQualityData data, CancellationToken cancellationToken = default)
    {
        await _dbContext.AirQualityData.AddAsync(data, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
