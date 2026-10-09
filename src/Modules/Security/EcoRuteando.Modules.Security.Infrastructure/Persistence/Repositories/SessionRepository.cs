using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly SecurityDbContext _context;

    public SessionRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(
                s => s.RefreshTokenHash == refreshTokenHash,
                cancellationToken);
    }

    public async Task<Session?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Session session,
        CancellationToken cancellationToken = default)
    {
        await _context.Sessions.AddAsync(session, cancellationToken);
    }

    public void Update(Session session)
    {
        _context.Sessions.Update(session);
    }
}
