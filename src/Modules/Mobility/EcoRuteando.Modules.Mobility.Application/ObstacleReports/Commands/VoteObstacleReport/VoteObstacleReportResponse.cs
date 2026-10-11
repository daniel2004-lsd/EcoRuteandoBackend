namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.VoteObstacleReport;

public sealed record VoteObstacleReportResponse(
    Guid ReportId,
    short ConfidenceScore,
    int ConfirmationsCount,
    int RejectionsCount,
    string Status,
    bool UserVote,
    string Message);
