using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class ErrorLogRepository : IErrorLogRepository
{
    private readonly SecurityDbContext _context;

    public ErrorLogRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ErrorLog errorLog,
        CancellationToken cancellationToken = default)
    {
        await _context.ErrorLogs.AddAsync(errorLog, cancellationToken);
    }

    public async Task<IReadOnlyList<ErrorLog>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ErrorLogs
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ErrorLog>> GetByLevelAsync(
        ErrorLevel level,
        CancellationToken cancellationToken = default)
    {
        return await _context.ErrorLogs
            .Where(e => e.ErrorLevel == level)
            .OrderByDescending(e => e.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
}
