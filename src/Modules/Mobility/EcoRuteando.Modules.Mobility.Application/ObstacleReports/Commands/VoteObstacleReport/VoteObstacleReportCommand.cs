using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.VoteObstacleReport;

public sealed record VoteObstacleReportCommand(
    Guid ReportId,
    bool Confirms,
    double? Latitude = null,
    double? Longitude = null,
    Guid UserId = default) : IRequest<VoteObstacleReportResponse>;
