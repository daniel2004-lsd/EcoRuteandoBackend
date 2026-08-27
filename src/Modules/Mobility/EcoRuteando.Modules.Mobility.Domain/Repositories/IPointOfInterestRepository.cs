using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IPointOfInterestRepository
{
    Task<PointOfInterest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los puntos de interés activos (para mostrarlos en el mapa).
    /// </summary>
    Task<IReadOnlyList<PointOfInterest>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
