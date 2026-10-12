using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Commands.UpsertDailyStatistic;

public class UpsertDailyStatisticCommandHandler : IRequestHandler<UpsertDailyStatisticCommand, DailyStatisticDto>
{
    private readonly IDailyStatisticRepository _repository;

    public UpsertDailyStatisticCommandHandler(IDailyStatisticRepository repository)
    {
        _repository = repository;
    }

    public async Task<DailyStatisticDto> Handle(UpsertDailyStatisticCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByDateAsync(request.StatisticDate, cancellationToken);
        if (existing == null)
        {
            existing = DailyStatistic.Create(
                request.StatisticDate,
                request.TotalUsers,
                request.NewUsers,
                request.ActiveUsers,
                request.TotalRoutes,
                request.CompletedRoutes,
                request.TotalCommunities,
                request.TotalReports,
                request.TotalCo2Saved,
                request.TotalDistanceKm);

            await _repository.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.UpdateMetrics(
                request.TotalUsers,
                request.NewUsers,
                request.ActiveUsers,
                request.TotalRoutes,
                request.CompletedRoutes,
                request.TotalCommunities,
                request.TotalReports,
                request.TotalCo2Saved,
                request.TotalDistanceKm);

            await _repository.UpdateAsync(existing, cancellationToken);
        }

        return new DailyStatisticDto(
            existing.Id,
            existing.StatisticDate,
            existing.TotalUsers,
            existing.NewUsers,
            existing.ActiveUsers,
            existing.TotalRoutes,
            existing.CompletedRoutes,
            existing.TotalCommunities,
            existing.TotalReports,
            existing.TotalCo2Saved,
            existing.TotalDistanceKm,
            existing.CreatedAt,
            existing.UpdatedAt);
    }
}
