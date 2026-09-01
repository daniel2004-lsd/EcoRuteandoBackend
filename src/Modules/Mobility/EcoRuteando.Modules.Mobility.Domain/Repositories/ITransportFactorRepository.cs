using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface ITransportFactorRepository
{
    Task<TransportFactor?> GetActiveByTransportTypeAsync(
        TransportType transportType,
        CancellationToken cancellationToken = default);
}
