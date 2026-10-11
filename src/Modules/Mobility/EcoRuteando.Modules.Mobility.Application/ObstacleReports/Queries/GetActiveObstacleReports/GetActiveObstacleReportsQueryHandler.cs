using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetActiveObstacleReports;

public sealed class GetActiveObstacleReportsQueryHandler
    : IRequestHandler<GetActiveObstacleReportsQuery, IReadOnlyList<ActiveObstacleReportResponse>>
{
    private readonly IObstacleReportRepository _reportRepository;

    public GetActiveObstacleReportsQueryHandler(IObstacleReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IReadOnlyList<ActiveObstacleReportResponse>> Handle(
        GetActiveObstacleReportsQuery request,
        CancellationToken cancellationToken)
    {
        var reports = await _reportRepository.GetActiveReportsAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            cancellationToken);

        if (reports.Count == 0)
        {
            return Array.Empty<ActiveObstacleReportResponse>();
        }

        var reportIds = reports.Select(r => r.Id).ToList();
        var voteStats = await _reportRepository.GetVotesStatsAsync(
            reportIds,
            request.CurrentUserId,
            cancellationToken);

        var result = new List<ActiveObstacleReportResponse>();
        var now = DateTime.UtcNow;

        foreach (var report in reports)
        {
            voteStats.TryGetValue(report.Id, out var stats);

            // Decaimiento por antigüedad: -1 punto cada 10 min desde UpdatedAt (CU22)
            var referenceDate = report.UpdatedAt ?? report.CreatedAt;
            int elapsedMinutes = Math.Max(0, (int)(now - referenceDate).TotalMinutes);
            int decay = elapsedMinutes / 10;
            short effectiveScore = (short)Math.Max(0, report.ConfidenceScore - decay);

            // El mapa solo muestra reportes con score efectivo >= 40
            if (effectiveScore < 40)
            {
                continue;
            }

            // Derivar estado comunitario
            string derivedStatus;
            if (report.ExpiresAt.HasValue && report.ExpiresAt.Value <= now)
            {
                derivedStatus = "EXPIRED";
            }
            else if (stats.Rejections > stats.Confirms)
            {
                derivedStatus = "DISPUTED";
            }
            else if (stats.Confirms >= 2 && effectiveScore >= 70)
            {
                derivedStatus = "CONFIRMED";
            }
            else
            {
                derivedStatus = "ACTIVE";
            }

            result.Add(new ActiveObstacleReportResponse(
                report.Id,
                report.ReportType,
                report.Description,
                report.Location.Y,
                report.Location.X,
                report.AddressText,
                report.PhotoUrl,
                report.Status.ToPgName(),
                effectiveScore,
                stats.Confirms,
                stats.Rejections,
                stats.UserVote,
                derivedStatus,
                report.CreatedAt,
                report.ExpiresAt));
        }

        return result;
    }
}
