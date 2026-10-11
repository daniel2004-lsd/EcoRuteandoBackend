namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetActiveObstacleReports;

public sealed record ActiveObstacleReportResponse(
    Guid Id,
    string ReportType,
    string Description,
    double Latitude,
    double Longitude,
    string? AddressText,
    string? PhotoUrl,
    string Status,
    short ConfidenceScore,
    int ConfirmationsCount,
    int RejectionsCount,
    bool? UserVote,
    string DerivedStatus,
    DateTime CreatedAt,
    DateTime? ExpiresAt);
