using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class TrustedDeviceRepository : ITrustedDeviceRepository
{
    private readonly SecurityDbContext _context;

    public TrustedDeviceRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TrustedDevice device,
        CancellationToken cancellationToken = default)
    {
        await _context.TrustedDevices.AddAsync(device, cancellationToken);
    }

    public async Task<TrustedDevice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .FirstOrDefaultAsync(td => td.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrustedDevice>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .Where(td => td.UserId == userId && !td.IsRevoked && td.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(td => td.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Update(TrustedDevice device)
    {
        _context.TrustedDevices.Update(device);
    }

    public async Task RevokeAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activeDevices = await _context.TrustedDevices
            .Where(td => td.UserId == userId && !td.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var device in activeDevices)
        {
            device.Revoke();
            _context.TrustedDevices.Update(device);
        }
    }
}
