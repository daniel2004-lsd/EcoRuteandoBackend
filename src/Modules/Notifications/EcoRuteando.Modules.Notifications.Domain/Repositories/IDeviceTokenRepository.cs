using EcoRuteando.Modules.Notifications.Domain.Entities;

namespace EcoRuteando.Modules.Notifications.Domain.Repositories;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByUserAndPlatformAsync(Guid userId, string platform, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceToken>> GetAllActiveTokensAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DeviceToken token, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
