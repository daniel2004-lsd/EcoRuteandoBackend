using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Notifications.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, bool? unreadOnly = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly.HasValue && unreadOnly.Value)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
