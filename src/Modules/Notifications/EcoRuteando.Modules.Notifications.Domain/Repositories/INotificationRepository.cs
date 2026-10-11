using EcoRuteando.Modules.Notifications.Domain.Entities;

namespace EcoRuteando.Modules.Notifications.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, bool? unreadOnly = null, int limit = 50, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
