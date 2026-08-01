using EcoRuteando.Modules.Security.Domain.Entities;

namespace EcoRuteando.Modules.Security.Domain.Repositories;

public interface IRolePermissionRepository
{
    Task<RolePermission?> GetAsync(
        int roleId,
        int permissionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RolePermission rolePermission,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        RolePermission rolePermission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(
        int roleId,
        CancellationToken cancellationToken = default);
}