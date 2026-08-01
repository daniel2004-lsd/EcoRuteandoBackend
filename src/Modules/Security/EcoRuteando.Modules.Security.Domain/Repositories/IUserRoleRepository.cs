using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IUserRoleRepository
{
    Task<UserRole?> GetAsync(
        Guid userId,
        int roleId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserRole>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}