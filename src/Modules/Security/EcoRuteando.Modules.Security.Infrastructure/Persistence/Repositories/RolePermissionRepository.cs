using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class RolePermissionRepository : IRolePermissionRepository
{
    private readonly SecurityDbContext _dbContext;

    public RolePermissionRepository(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RolePermission?> GetAsync(
        int roleId,
        int permissionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RolePermissions
            .FirstOrDefaultAsync(
                rp => rp.RoleId == roleId &&
                      rp.PermissionId == permissionId,
                cancellationToken);
    }

    public async Task AddAsync(
        RolePermission rolePermission,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RolePermissions.AddAsync(
            rolePermission,
            cancellationToken);
    }

    public Task DeleteAsync(
        RolePermission rolePermission,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RolePermissions.Remove(rolePermission);

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}