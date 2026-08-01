using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly SecurityDbContext _dbContext;

    public RefreshTokenRepository(SecurityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task<RefreshToken?> GetByHashAsync(
     string tokenHash,
     CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.PrimaryRole)
            .FirstOrDefaultAsync(
                rt => rt.TokenHash == tokenHash,
                cancellationToken);
    }

    public Task UpdateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
    }
  
}