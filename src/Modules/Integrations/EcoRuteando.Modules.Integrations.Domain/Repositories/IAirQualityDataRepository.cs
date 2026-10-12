using EcoRuteando.Modules.Integrations.Domain.Entities;

namespace EcoRuteando.Modules.Integrations.Domain.Repositories;

public interface IAirQualityDataRepository
{
    Task<AirQualityData?> GetLatestByCoordinatesAsync(double latitude, double longitude, double maxDistanceMeters = 50000, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AirQualityData>> GetRecentByCityAsync(string city, int limit = 10, CancellationToken cancellationToken = default);
    Task AddAsync(AirQualityData data, CancellationToken cancellationToken = default);
}
