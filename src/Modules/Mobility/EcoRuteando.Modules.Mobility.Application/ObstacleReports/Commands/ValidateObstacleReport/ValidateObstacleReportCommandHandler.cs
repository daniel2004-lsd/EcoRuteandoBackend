using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.ValidateObstacleReport;

/// <summary>
/// Validación administrativa de un reporte de obstáculo (CU07, flujo del actor
/// Administrador): lo deja 'validated' o 'rejected' con una nota opcional.
/// </summary>
public sealed class ValidateObstacleReportCommandHandler
    : IRequestHandler<ValidateObstacleReportCommand>
{
    private readonly IObstacleReportRepository _reportRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public ValidateObstacleReportCommandHandler(
        IObstacleReportRepository reportRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ValidateObstacleReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!PgEnumExtensions.TryFromPgName(request.Status, out ReportStatus status)
            || status is not (ReportStatus.Validated or ReportStatus.Rejected))
        {
            throw new DomainException("El reporte solo puede quedar validado o rechazado.");
        }

        var report = await _reportRepository.GetByIdAsync(
            request.ReportId,
            cancellationToken);

        if (report is null)
        {
            throw new NotFoundException("El reporte no existe.");
        }

        report.Validate(
            request.ValidatorId,
            status,
            request.ValidationNote);

        _reportRepository.Update(report);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}