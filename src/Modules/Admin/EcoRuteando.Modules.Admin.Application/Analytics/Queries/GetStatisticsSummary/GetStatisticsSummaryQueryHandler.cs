using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Queries.GetStatisticsSummary;

public class GetStatisticsSummaryQueryHandler : IRequestHandler<GetStatisticsSummaryQuery, StatisticsSummaryDto>
{
    private readonly IDailyStatisticRepository _repository;

    public GetStatisticsSummaryQueryHandler(IDailyStatisticRepository repository)
    {
        _repository = repository;
    }

    public async Task<StatisticsSummaryDto> Handle(GetStatisticsSummaryQuery request, CancellationToken cancellationToken)
    {
        var latest = await _repository.GetLatestAsync(cancellationToken);

        if (latest == null)
        {
            return new StatisticsSummaryDto(0, 0, 0, 0, 0, 0, 0, 0, null);
        }

        return new StatisticsSummaryDto(
            latest.TotalUsers,
            latest.ActiveUsers,
            latest.TotalRoutes,
            latest.CompletedRoutes,
            latest.TotalCommunities,
            latest.TotalReports,
            latest.TotalCo2Saved,
            latest.TotalDistanceKm,
            latest.StatisticDate);
    }
}
