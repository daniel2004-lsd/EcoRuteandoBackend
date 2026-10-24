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

    /// <summary>
    /// Devuelve todas las rutas (activas, inactivas, en revisión o archivadas).
    /// Se usa en la gestión de rutas (HU-12 / CU03).
    /// </summary>
    Task<IReadOnlyList<Route>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta paginada y filtrada a nivel de base de datos para aislamiento y rendimiento.
    /// </summary>
    Task<IReadOnlyList<Route>> GetRoutesAsync(
        TransportType? transportType = null,
        bool includeInactive = false,
        Guid? createdBy = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba si ya existe una ruta con el mismo nombre (regla de negocio CU03:
    /// no permitir rutas duplicadas). <paramref name="excludeId"/> permite excluir
    /// la propia ruta en una actualización.
    /// </summary>
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Route route,
        CancellationToken cancellationToken = default);

    void Update(Route route);

    void Delete(Route route);
}
