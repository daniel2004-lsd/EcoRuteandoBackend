using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface ISecurityPolicyRepository
{
    Task<SecurityPolicy?> GetAsync(CancellationToken cancellationToken = default);

    Task AddAsync(SecurityPolicy policy, CancellationToken cancellationToken = default);

    void Update(SecurityPolicy policy);
}
