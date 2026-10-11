using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Queries.GetDailyStatistics;

public record GetDailyStatisticsQuery(DateOnly? From = null, DateOnly? To = null) : IRequest<IReadOnlyList<DailyStatisticDto>>;
