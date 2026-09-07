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