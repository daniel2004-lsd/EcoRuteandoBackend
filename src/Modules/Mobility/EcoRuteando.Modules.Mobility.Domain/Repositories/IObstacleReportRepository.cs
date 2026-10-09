using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;

namespace EcoRuteando.Modules.Mobility.Domain.Repositories;

public interface IObstacleReportRepository
{
    Task<ObstacleReport?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ObstacleReport>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ObstacleReport>> GetForValidationAsync(
        ReportStatus? status = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ObstacleReport report,
        CancellationToken cancellationToken = default);

    void Update(ObstacleReport report);
}