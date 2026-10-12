using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Analytics.Commands.UpsertDailyStatistic;

public record UpsertDailyStatisticCommand(
    DateOnly StatisticDate,
    int TotalUsers,
    int NewUsers,
    int ActiveUsers,
    int TotalRoutes,
    int CompletedRoutes,
    int TotalCommunities,
    int TotalReports,
    decimal TotalCo2Saved,
    decimal TotalDistanceKm) : IRequest<DailyStatisticDto>;
