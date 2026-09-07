using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;

public sealed record CreateObstacleReportCommand(
    string ReportType,
    string Description,
    double Latitude,
    double Longitude,
    string? AddressText,
    Guid UserId)
    : IRequest<Guid>;