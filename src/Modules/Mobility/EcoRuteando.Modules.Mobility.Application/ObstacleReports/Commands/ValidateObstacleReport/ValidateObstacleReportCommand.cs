using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.ValidateObstacleReport;

/// <summary>
/// Status se recibe como label de PostgreSQL ('validated'/'rejected')
/// y se convierte a <see cref="ReportStatus"/> en el handler.
/// </summary>
public sealed record ValidateObstacleReportCommand(
    Guid ReportId,
    string Status,
    string? ValidationNote,
    Guid ValidatorId)
    : IRequest;