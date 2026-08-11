using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;

public sealed class PasswordRecoveryRepository : IPasswordRecoveryRepository
{
    private readonly SecurityDbContext _context;

    public PasswordRecoveryRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordRecovery recovery)
    {
        await _context.PasswordRecoveries.AddAsync(recovery);
    }

    public async Task<PasswordRecovery?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.PasswordRecoveries
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task<IEnumerable<PasswordRecovery>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PasswordRecoveries
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public void Update(PasswordRecovery recovery)
    {
        _context.PasswordRecoveries.Update(recovery);
    }

    public async Task<PasswordRecovery?> GetByIdAsync(Guid id)
    {
        return await _context.PasswordRecoveries
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Delete(PasswordRecovery passwordRecovery)
    {
        _context.PasswordRecoveries.Remove(passwordRecovery);
    }

}