using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Role role,
        CancellationToken cancellationToken = default);
}
