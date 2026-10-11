using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public class DailyStatisticRepository : IDailyStatisticRepository
{
    private readonly AdminDbContext _dbContext;

    public DailyStatisticRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DailyStatistic>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyStatistics
            .AsNoTracking()
            .Where(s => s.StatisticDate >= from && s.StatisticDate <= to)
            .OrderBy(s => s.StatisticDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<DailyStatistic?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyStatistics
            .FirstOrDefaultAsync(s => s.StatisticDate == date, cancellationToken);
    }

    public async Task<DailyStatistic?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.DailyStatistics
            .AsNoTracking()
            .OrderByDescending(s => s.StatisticDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(DailyStatistic statistic, CancellationToken cancellationToken = default)
    {
        await _dbContext.DailyStatistics.AddAsync(statistic, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DailyStatistic statistic, CancellationToken cancellationToken = default)
    {
        _dbContext.DailyStatistics.Update(statistic);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
