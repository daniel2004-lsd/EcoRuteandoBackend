using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class TwoFactorAuthRepository : ITwoFactorAuthRepository
{
    private readonly SecurityDbContext _context;

    public TwoFactorAuthRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<TwoFactorAuth?> GetByUserIdAndMethodAsync(
        Guid userId,
        TwoFactorMethod method,
        CancellationToken cancellationToken = default)
    {
        return await _context.TwoFactorAuths
            .FirstOrDefaultAsync(
                t => t.UserId == userId && t.Method == method,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TwoFactorAuth>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TwoFactorAuths
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TwoFactorAuth twoFactorAuth,
        CancellationToken cancellationToken = default)
    {
        await _context.TwoFactorAuths.AddAsync(twoFactorAuth, cancellationToken);
    }

    public void Update(TwoFactorAuth twoFactorAuth)
    {
        _context.TwoFactorAuths.Update(twoFactorAuth);
    }
}
