using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Queries.GetDailyStatistics;

public class GetDailyStatisticsQueryHandler : IRequestHandler<GetDailyStatisticsQuery, IReadOnlyList<DailyStatisticDto>>
{
    private readonly IDailyStatisticRepository _repository;

    public GetDailyStatisticsQueryHandler(IDailyStatisticRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DailyStatisticDto>> Handle(GetDailyStatisticsQuery request, CancellationToken cancellationToken)
    {
        var to = request.To ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = request.From ?? to.AddDays(-30);

        var list = await _repository.GetByDateRangeAsync(from, to, cancellationToken);

        return list.Select(s => new DailyStatisticDto(
            s.Id,
            s.StatisticDate,
            s.TotalUsers,
            s.NewUsers,
            s.ActiveUsers,
            s.TotalRoutes,
            s.CompletedRoutes,
            s.TotalCommunities,
            s.TotalReports,
            s.TotalCo2Saved,
            s.TotalDistanceKm,
            s.CreatedAt,
            s.UpdatedAt
        )).ToList();
    }
}
