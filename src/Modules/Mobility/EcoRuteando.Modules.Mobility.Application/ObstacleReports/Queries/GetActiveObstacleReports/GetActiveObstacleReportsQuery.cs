using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetActiveObstacleReports;

public sealed record GetActiveObstacleReportsQuery(
    double? Latitude = null,
    double? Longitude = null,
    double? RadiusKm = null,
    Guid? CurrentUserId = null) : IRequest<IReadOnlyList<ActiveObstacleReportResponse>>;
