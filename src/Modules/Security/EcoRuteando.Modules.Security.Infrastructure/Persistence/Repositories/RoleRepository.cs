using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly SecurityDbContext _dbContext;
   
    public RoleRepository(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .FirstOrDefaultAsync(
                r => r.Name == name,
                cancellationToken);
    }

    public async Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Roles.AddAsync(role, cancellationToken);
       
    }

    public Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Update(role);
        return Task.CompletedTask;
    }

    public  Task DeleteAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Roles.Remove(role);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}