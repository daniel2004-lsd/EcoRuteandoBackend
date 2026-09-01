using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class TransportFactorRepository : ITransportFactorRepository
{
    private readonly MobilityDbContext _dbContext;

    public TransportFactorRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransportFactor?> GetActiveByTransportTypeAsync(
        TransportType transportType,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.TransportFactors
            .Where(tf => tf.TransportType == transportType)
            .Where(tf => tf.ValidFrom <= now)
            .Where(tf => tf.ValidUntil == null || tf.ValidUntil > now)
            .OrderByDescending(tf => tf.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
