using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface ITransportFactorRepository
{
    Task<IReadOnlyList<TransportFactor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransportFactor>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<TransportFactor?> GetActiveByTransportTypeAsync(TransportType transportType, CancellationToken cancellationToken = default);
    Task<TransportFactor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TransportFactor factor, CancellationToken cancellationToken = default);
    Task UpdateAsync(TransportFactor factor, CancellationToken cancellationToken = default);
}
