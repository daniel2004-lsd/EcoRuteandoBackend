using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IOAuthAccountRepository
{
    Task<OAuthAccount?> GetByProviderAndExternalIdAsync(
        OAuthProvider provider,
        string externalId,
        CancellationToken cancellationToken = default);

    Task<OAuthAccount?> GetByUserIdAndProviderAsync(
        Guid userId,
        OAuthProvider provider,
        CancellationToken cancellationToken = default);

    Task AddAsync(OAuthAccount account, CancellationToken cancellationToken = default);

    void Update(OAuthAccount account);
}
