using EcoRuteando.Modules.Security.Domain.Entities;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(
        int id,
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