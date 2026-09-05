using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IRatingRepository
{
    Task<Rating?> GetByUserAndRouteAsync(
        Guid userId,
        Guid routeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Rating>> GetByRouteAsync(
        Guid routeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Rating rating,
        CancellationToken cancellationToken = default);

    void Update(Rating rating);
}
