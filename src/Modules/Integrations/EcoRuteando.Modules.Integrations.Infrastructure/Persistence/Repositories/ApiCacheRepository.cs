using EcoRuteando.Modules.Integrations.Domain.Entities;
using EcoRuteando.Modules.Integrations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Repositories;

public class ApiCacheRepository : IApiCacheRepository
{
    private readonly IntegrationsDbContext _dbContext;

    public ApiCacheRepository(IntegrationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiCacheEntry?> GetValidEntryAsync(string provider, string endpoint, string parametersHash, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.ApiCache
            .AsNoTracking()
            .Where(c => c.Provider == provider
                     && c.Endpoint == endpoint
                     && c.ParametersHash == parametersHash
                     && c.IsValid
                     && c.ExpiresAt > now)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApiCacheEntry>> GetRecentEntriesAsync(string? provider = null, int limit = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ApiCache.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(c => c.Provider == provider);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(Math.Clamp(limit, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ApiCacheEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApiCache.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateAsync(long id, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.ApiCache.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entry != null)
        {
            entry.Invalidate();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task InvalidateByProviderAsync(string provider, CancellationToken cancellationToken = default)
    {
        var entries = await _dbContext.ApiCache
            .Where(c => c.Provider == provider && c.IsValid)
            .ToListAsync(cancellationToken);

        foreach (var entry in entries)
        {
            entry.Invalidate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expired = await _dbContext.ApiCache
            .Where(c => c.ExpiresAt <= now || !c.IsValid)
            .ToListAsync(cancellationToken);

        _dbContext.ApiCache.RemoveRange(expired);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }
}
