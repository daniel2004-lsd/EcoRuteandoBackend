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

    Task<IReadOnlyList<ObstacleReport>> GetActiveReportsAsync(
        double? latitude = null,
        double? longitude = null,
        double? radiusKm = null,
        CancellationToken cancellationToken = default);

    Task<ReportVote?> GetVoteAsync(
        Guid reportId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReportVote>> GetVotesForReportAsync(
        Guid reportId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, (int Confirms, int Rejections, bool? UserVote)>> GetVotesStatsAsync(
        IEnumerable<Guid> reportIds,
        Guid? currentUserId = null,
        CancellationToken cancellationToken = default);

    Task AddVoteAsync(
        ReportVote vote,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ObstacleReport report,
        CancellationToken cancellationToken = default);

    void Update(ObstacleReport report);
}