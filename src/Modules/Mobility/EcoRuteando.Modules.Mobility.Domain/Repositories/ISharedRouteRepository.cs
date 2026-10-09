using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface ISharedRouteRepository
{
    Task<SharedRoute?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SharedRoute>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SharedRoute sharedRoute,
        CancellationToken cancellationToken = default);

    void Delete(SharedRoute sharedRoute);
}