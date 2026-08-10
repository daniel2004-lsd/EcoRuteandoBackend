using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly SecurityDbContext _dbContext;

    public UserRoleRepository(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserRole?> GetAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .FirstOrDefaultAsync(
                ur => ur.UserId == userId &&
                      ur.RoleId == roleId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.UserRoles.AddAsync(
            userRole,
            cancellationToken);
    }

    public Task DeleteAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        _dbContext.UserRoles.Remove(userRole);

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}