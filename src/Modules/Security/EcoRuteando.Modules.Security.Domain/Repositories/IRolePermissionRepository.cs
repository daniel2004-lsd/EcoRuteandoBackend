using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IRolePermissionRepository
{
    Task<RolePermission?> GetAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RolePermission rolePermission,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        RolePermission rolePermission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);
}