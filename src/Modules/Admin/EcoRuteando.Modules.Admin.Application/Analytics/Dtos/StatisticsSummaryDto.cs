namespace EcoRuteando.Modules.Admin.Application.Analytics.Dtos;

public record StatisticsSummaryDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalRoutes,
    int CompletedRoutes,
    int TotalCommunities,
    int TotalReports,
    decimal TotalCo2SavedKg,
    decimal TotalDistanceKm,
    DateOnly? LastCalculatedDate);
