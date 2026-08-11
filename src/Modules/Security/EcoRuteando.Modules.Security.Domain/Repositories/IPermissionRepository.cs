using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Permission?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Permission?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Permission permission,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Permission permission,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Permission permission,
        CancellationToken cancellationToken = default);
}