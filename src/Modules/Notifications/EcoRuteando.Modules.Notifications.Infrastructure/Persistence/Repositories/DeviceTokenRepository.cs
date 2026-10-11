using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Notifications.Infrastructure.Persistence.Repositories;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly NotificationsDbContext _context;

    public DeviceTokenRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceToken?> GetByUserAndPlatformAsync(Guid userId, string platform, CancellationToken cancellationToken = default)
    {
        return await _context.DeviceTokens
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Platform == platform.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.DeviceTokens
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceToken>> GetAllActiveTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeviceTokens
            .Where(d => d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DeviceToken token, CancellationToken cancellationToken = default)
    {
        await _context.DeviceTokens.AddAsync(token, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
