using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class OAuthAccountRepository : IOAuthAccountRepository
{
    private readonly SecurityDbContext _context;

    public OAuthAccountRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<OAuthAccount?> GetByProviderAndExternalIdAsync(
        OAuthProvider provider,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.OAuthAccounts
            .Include(o => o.User)
            .ThenInclude(u => u.PrimaryRole)
            .FirstOrDefaultAsync(
                o => o.Provider == provider && o.ExternalId == externalId,
                cancellationToken);
    }

    public async Task<OAuthAccount?> GetByUserIdAndProviderAsync(
        Guid userId,
        OAuthProvider provider,
        CancellationToken cancellationToken = default)
    {
        return await _context.OAuthAccounts
            .FirstOrDefaultAsync(
                o => o.UserId == userId && o.Provider == provider,
                cancellationToken);
    }

    public async Task AddAsync(
        OAuthAccount account,
        CancellationToken cancellationToken = default)
    {
        await _context.OAuthAccounts.AddAsync(account, cancellationToken);
    }

    public void Update(OAuthAccount account)
    {
        _context.OAuthAccounts.Update(account);
    }
}
