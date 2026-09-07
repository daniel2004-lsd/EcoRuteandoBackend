using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetObstacleReportsForValidation;

public sealed class GetObstacleReportsForValidationQueryHandler
    : IRequestHandler<GetObstacleReportsForValidationQuery, IReadOnlyList<ObstacleReportResponse>>
{
    private readonly IObstacleReportRepository _reportRepository;

    public GetObstacleReportsForValidationQueryHandler(
        IObstacleReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IReadOnlyList<ObstacleReportResponse>> Handle(
        GetObstacleReportsForValidationQuery request,
        CancellationToken cancellationToken)
    {
        var reports = await _reportRepository.GetForValidationAsync(
            request.Status,
            cancellationToken);

        return reports
            .Select(r => new ObstacleReportResponse(
                r.Id,
                r.ReportType,
                r.Description,
                r.Location.Y,
                r.Location.X,
                r.AddressText,
                r.PhotoUrl,
                r.Status.ToPgName(),
                r.ValidationNote,
                r.ValidatedAt,
                r.CreatedAt))
            .ToList();
    }
}