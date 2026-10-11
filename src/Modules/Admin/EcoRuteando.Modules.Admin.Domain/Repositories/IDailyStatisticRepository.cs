using EcoRuteando.Modules.Admin.Domain.Entities;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface IDailyStatisticRepository
{
    Task<IReadOnlyList<DailyStatistic>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<DailyStatistic?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyStatistic?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DailyStatistic statistic, CancellationToken cancellationToken = default);
    Task UpdateAsync(DailyStatistic statistic, CancellationToken cancellationToken = default);
}
