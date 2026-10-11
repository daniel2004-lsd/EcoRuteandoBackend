using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class ObstacleReportRepository : IObstacleReportRepository
{
    private readonly MobilityDbContext _dbContext;

    public ObstacleReportRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ObstacleReport?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ObstacleReports
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == id && r.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ObstacleReport>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ObstacleReports
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ObstacleReport>> GetForValidationAsync(
        ReportStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ObstacleReport> query = _dbContext.ObstacleReports
            .AsNoTracking()
            .Where(r => r.IsActive);

        if (status is not null)
        {
            query = query.Where(r => r.Status == status);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ObstacleReport>> GetActiveReportsAsync(
        double? latitude = null,
        double? longitude = null,
        double? radiusKm = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        IQueryable<ObstacleReport> query = _dbContext.ObstacleReports
            .AsNoTracking()
            .Where(r => r.IsActive
                && r.Status != ReportStatus.Rejected
                && (r.ExpiresAt == null || r.ExpiresAt > now)
                && r.ConfidenceScore >= 40);

        if (latitude.HasValue && longitude.HasValue && radiusKm.HasValue && radiusKm.Value > 0)
        {
            var userPoint = new NetTopologySuite.Geometries.Point(longitude.Value, latitude.Value) { SRID = 4326 };
            double meters = radiusKm.Value * 1000.0;
            query = query.Where(r => r.Location.IsWithinDistance(userPoint, meters));
        }

        return await query
            .OrderByDescending(r => r.ConfidenceScore)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReportVote?> GetVoteAsync(
        Guid reportId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReportVotes
            .FirstOrDefaultAsync(v => v.ReportId == reportId && v.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ReportVote>> GetVotesForReportAsync(
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReportVotes
            .AsNoTracking()
            .Where(v => v.ReportId == reportId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, (int Confirms, int Rejections, bool? UserVote)>> GetVotesStatsAsync(
        IEnumerable<Guid> reportIds,
        Guid? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        var idsList = reportIds.ToList();
        if (idsList.Count == 0)
            return new();

        var votes = await _dbContext.ReportVotes
            .AsNoTracking()
            .Where(v => idsList.Contains(v.ReportId))
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, (int Confirms, int Rejections, bool? UserVote)>();

        foreach (var id in idsList)
        {
            var reportVotes = votes.Where(v => v.ReportId == id).ToList();
            int confirms = reportVotes.Count(v => v.Confirms);
            int rejections = reportVotes.Count(v => !v.Confirms);
            bool? userVote = currentUserId.HasValue
                ? reportVotes.FirstOrDefault(v => v.UserId == currentUserId.Value)?.Confirms
                : null;

            result[id] = (confirms, rejections, userVote);
        }

        return result;
    }

    public async Task AddVoteAsync(
        ReportVote vote,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ReportVotes.AddAsync(vote, cancellationToken);
    }

    public async Task AddAsync(
        ObstacleReport report,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ObstacleReports.AddAsync(report, cancellationToken);
    }

    public void Update(ObstacleReport report)
    {
        _dbContext.ObstacleReports.Update(report);
    }
}