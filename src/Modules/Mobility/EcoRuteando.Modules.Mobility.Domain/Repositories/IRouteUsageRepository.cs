using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IRouteUsageRepository
{
    Task<RouteUsage?> GetByIdAsync(
        Guid usageId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RouteUsage>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RouteUsage routeUsage,
        CancellationToken cancellationToken = default);

    void Update(RouteUsage routeUsage);
}
