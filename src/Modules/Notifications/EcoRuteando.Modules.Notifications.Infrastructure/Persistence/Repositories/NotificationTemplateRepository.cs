using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Notifications.Infrastructure.Persistence.Repositories;

public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationTemplateRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<NotificationTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationTemplate>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.NotificationTemplates.AsQueryable();
        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.NotificationTemplates.AddAsync(template, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
