using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Repositories;

public class WeatherDataRepository : IWeatherDataRepository
{
    private readonly IntegrationsDbContext _dbContext;

    public WeatherDataRepository(IntegrationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeatherData?> GetCurrentByCoordinatesAsync(double latitude, double longitude, double maxDistanceMeters = 50000, CancellationToken cancellationToken = default)
    {
        var target = new Point(longitude, latitude) { SRID = 4326 };
        var now = DateTime.UtcNow;

        return await _dbContext.WeatherData
            .AsNoTracking()
            .Where(w => w.ValidFrom <= now && w.ValidUntil >= now && w.Location.IsWithinDistance(target, maxDistanceMeters))
            .OrderByDescending(w => w.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherData>> GetRecentByCityAsync(string city, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WeatherData
            .AsNoTracking()
            .Where(w => w.City != null && EF.Functions.ILike(w.City, $"%{city}%"))
            .OrderByDescending(w => w.ValidFrom)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WeatherData data, CancellationToken cancellationToken = default)
    {
        await _dbContext.WeatherData.AddAsync(data, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
