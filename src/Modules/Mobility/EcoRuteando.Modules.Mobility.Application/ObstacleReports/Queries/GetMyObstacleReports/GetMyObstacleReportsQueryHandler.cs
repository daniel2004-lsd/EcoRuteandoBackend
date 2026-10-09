using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetMyObstacleReports;

public sealed class GetMyObstacleReportsQueryHandler
    : IRequestHandler<GetMyObstacleReportsQuery, IReadOnlyList<ObstacleReportResponse>>
{
    private readonly IObstacleReportRepository _reportRepository;

    public GetMyObstacleReportsQueryHandler(
        IObstacleReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IReadOnlyList<ObstacleReportResponse>> Handle(
        GetMyObstacleReportsQuery request,
        CancellationToken cancellationToken)
    {
        var reports = await _reportRepository.GetMineAsync(
            request.UserId,
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