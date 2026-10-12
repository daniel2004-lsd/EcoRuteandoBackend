using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public class AdminExportRepository : IAdminExportRepository
{
    private readonly AdminDbContext _dbContext;

    public AdminExportRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminExport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Exports
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminExport>> GetRecentAsync(Guid? userId = null, int limit = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Exports.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(e => e.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Take(Math.Clamp(limit, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AdminExport export, CancellationToken cancellationToken = default)
    {
        await _dbContext.Exports.AddAsync(export, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AdminExport export, CancellationToken cancellationToken = default)
    {
        _dbContext.Exports.Update(export);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
