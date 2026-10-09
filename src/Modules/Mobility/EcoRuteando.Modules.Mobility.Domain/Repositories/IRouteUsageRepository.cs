using EcoRuteando.Modules.Mobility.Domain.Entities;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IRouteUsageRepository
{
    Task<RouteUsage?> GetByIdAsync(
        Guid usageId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RouteUsage>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si el usuario ha completado al menos un trayecto de la ruta
    /// (precondición de la HU-11 "Calificar rutas").
    /// </summary>
    Task<bool> HasCompletedUsageAsync(
        Guid userId,
        Guid routeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega los trayectos completados en un rango de fechas (opcional)
    /// para el portal de estadísticas (HU-13 / CU08).
    /// </summary>
    Task<RouteUsageAnalytics> GetAnalyticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RouteUsage routeUsage,
        CancellationToken cancellationToken = default);

    void Update(RouteUsage routeUsage);
}
