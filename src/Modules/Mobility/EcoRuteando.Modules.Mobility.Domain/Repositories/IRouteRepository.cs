using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IRouteRepository
{
    Task<Route?> GetWithPoisByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Route>> GetActiveAsync(
        TransportType? transportType = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Route route,
        CancellationToken cancellationToken = default);

    void Update(Route route);

    void Delete(Route route);
}
