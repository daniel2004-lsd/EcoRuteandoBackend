using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface ITrustedDeviceRepository
{
    Task AddAsync(
        TrustedDevice device,
        CancellationToken cancellationToken = default);

    Task<TrustedDevice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrustedDevice>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    void Update(TrustedDevice device);

    Task RevokeAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
