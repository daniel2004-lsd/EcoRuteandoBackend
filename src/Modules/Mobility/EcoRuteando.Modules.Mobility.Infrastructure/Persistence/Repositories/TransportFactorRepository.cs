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

        // Cast al enum de Admin si se compara con la entidad base
        var adminType = (EcoRuteando.Modules.Admin.Domain.Enums.TransportType)(int)transportType;

        return await _dbContext.TransportFactors
            .AsNoTracking()
            .Where(tf => tf.TransportType == adminType
                         && tf.ValidFrom <= now
                         && (tf.ValidUntil == null || tf.ValidUntil > now))
            .OrderByDescending(tf => tf.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
