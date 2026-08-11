using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly SecurityDbContext _dbContext;

    public PermissionRepository(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<Permission?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .FirstOrDefaultAsync(
                p => p.Name == name,
                cancellationToken);
    }

    public async Task AddAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Permissions.AddAsync(
            permission,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Permissions.Update(permission);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Permissions.Remove(permission);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}