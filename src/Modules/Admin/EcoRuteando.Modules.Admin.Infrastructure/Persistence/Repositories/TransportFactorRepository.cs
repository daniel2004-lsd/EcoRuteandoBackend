using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public class TransportFactorRepository : ITransportFactorRepository
{
    private readonly AdminDbContext _dbContext;

    public TransportFactorRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TransportFactor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TransportFactors
            .AsNoTracking()
            .OrderBy(f => f.TransportType)
            .ThenByDescending(f => f.ValidFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TransportFactor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.TransportFactors
            .AsNoTracking()
            .Where(f => f.ValidFrom <= now && (!f.ValidUntil.HasValue || f.ValidUntil.Value > now))
            .OrderBy(f => f.TransportType)
            .ToListAsync(cancellationToken);
    }

    public async Task<TransportFactor?> GetActiveByTransportTypeAsync(TransportType transportType, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.TransportFactors
            .Where(f => f.TransportType == transportType && f.ValidFrom <= now && (!f.ValidUntil.HasValue || f.ValidUntil.Value > now))
            .OrderByDescending(f => f.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TransportFactor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TransportFactors
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task AddAsync(TransportFactor factor, CancellationToken cancellationToken = default)
    {
        await _dbContext.TransportFactors.AddAsync(factor, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TransportFactor factor, CancellationToken cancellationToken = default)
    {
        _dbContext.TransportFactors.Update(factor);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
