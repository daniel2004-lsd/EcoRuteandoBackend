using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;

/// <summary>
/// Handler del CU07 (HU-16 "Reporte ciudadano de obstáculos").
/// Guarda un reporte de obstáculo en estado 'pending' para validación.
/// </summary>
public sealed class CreateObstacleReportCommandHandler
    : IRequestHandler<CreateObstacleReportCommand, Guid>
{
    private readonly IObstacleReportRepository _reportRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public CreateObstacleReportCommandHandler(
        IObstacleReportRepository reportRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateObstacleReportCommand request,
        CancellationToken cancellationToken)
    {
        // En NetTopologySuite: X = longitud, Y = latitud
        var location = new Point(request.Longitude, request.Latitude);

        var report = new ObstacleReport(
            request.UserId,
            request.ReportType,
            request.Description,
            location,
            request.AddressText);

        await _reportRepository.AddAsync(report, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return report.Id;
    }
}