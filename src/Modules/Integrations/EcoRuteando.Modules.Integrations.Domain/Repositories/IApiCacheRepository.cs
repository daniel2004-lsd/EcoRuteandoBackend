using EcoRuteando.Modules.Integrations.Domain.Entities;

namespace EcoRuteando.Modules.Integrations.Domain.Repositories;

public interface IApiCacheRepository
{
    Task<ApiCacheEntry?> GetValidEntryAsync(string provider, string endpoint, string parametersHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiCacheEntry>> GetRecentEntriesAsync(string? provider = null, int limit = 20, CancellationToken cancellationToken = default);
    Task AddAsync(ApiCacheEntry entry, CancellationToken cancellationToken = default);
    Task InvalidateAsync(long id, CancellationToken cancellationToken = default);
    Task InvalidateByProviderAsync(string provider, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default);
}
