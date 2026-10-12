using EcoRuteando.Modules.Integrations.Domain.Entities;

namespace EcoRuteando.Modules.Integrations.Domain.Repositories;

public interface IWeatherDataRepository
{
    Task<WeatherData?> GetCurrentByCoordinatesAsync(double latitude, double longitude, double maxDistanceMeters = 50000, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WeatherData>> GetRecentByCityAsync(string city, int limit = 10, CancellationToken cancellationToken = default);
    Task AddAsync(WeatherData data, CancellationToken cancellationToken = default);
}
