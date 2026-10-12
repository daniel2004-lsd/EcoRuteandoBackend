namespace EcoRuteando.Modules.Admin.Application.Analytics.Dtos;

public record DailyStatisticDto(
    Guid Id,
    DateOnly StatisticDate,
    int TotalUsers,
    int NewUsers,
    int ActiveUsers,
    int TotalRoutes,
    int CompletedRoutes,
    int TotalCommunities,
    int TotalReports,
    decimal TotalCo2Saved,
    decimal TotalDistanceKm,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
