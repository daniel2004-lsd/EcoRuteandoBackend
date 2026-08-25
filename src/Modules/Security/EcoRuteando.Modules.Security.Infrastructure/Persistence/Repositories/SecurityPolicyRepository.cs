using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class SecurityPolicyRepository : ISecurityPolicyRepository
{
    private readonly SecurityDbContext _context;

    public SecurityPolicyRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<SecurityPolicy?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SecurityPolicies
            .FirstOrDefaultAsync(p => p.Id == 1, cancellationToken);
    }

    public async Task AddAsync(
        SecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        await _context.SecurityPolicies.AddAsync(policy, cancellationToken);
    }

    public void Update(SecurityPolicy policy)
    {
        _context.SecurityPolicies.Update(policy);
    }
}
