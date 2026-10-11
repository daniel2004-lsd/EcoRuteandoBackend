using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.VoteObstacleReport;

/// <summary>
/// Handler para el caso de uso CU22: Validación comunitaria de reportes de obstáculos.
/// Permite confirmar o rechazar la presencia de un obstáculo y recalcula el nivel de confianza.
/// </summary>
public sealed class VoteObstacleReportCommandHandler
    : IRequestHandler<VoteObstacleReportCommand, VoteObstacleReportResponse>
{
    private readonly IObstacleReportRepository _reportRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public VoteObstacleReportCommandHandler(
        IObstacleReportRepository reportRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VoteObstacleReportResponse> Handle(
        VoteObstacleReportCommand request,
        CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.ReportId, cancellationToken);
        if (report is null)
        {
            throw new NotFoundException("El reporte de obstáculo no existe o fue dado de baja.");
        }

        // Validación de proximidad si el cliente envía su ubicación (Excepción E2: <= 500 metros)
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            double reportLat = report.Location.Y;
            double reportLng = report.Location.X;
            double distanceMeters = CalculateDistanceMeters(
                request.Latitude.Value,
                request.Longitude.Value,
                reportLat,
                reportLng);

            if (distanceMeters > 500.0)
            {
                throw new DomainException("Debes estar a menos de 500 metros del reporte para confirmar o rechazar su estado.");
            }
        }

        // Buscar voto previo del usuario (Excepción E3: actualizar voto si ya existía)
        var existingVote = await _reportRepository.GetVoteAsync(
            request.ReportId,
            request.UserId,
            cancellationToken);

        if (existingVote is not null)
        {
            existingVote.UpdateVote(request.Confirms);
        }
        else
        {
            var newVote = new ReportVote(request.UserId, request.ReportId, request.Confirms);
            await _reportRepository.AddVoteAsync(newVote, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recalcular nivel de confianza (confidence_score) y estado
        var allVotes = await _reportRepository.GetVotesForReportAsync(request.ReportId, cancellationToken);

        // Excepción E1: Voto del creador se registra pero no aporta puntos de confirmación independiente
        int independentConfirms = allVotes.Count(v => v.Confirms && v.UserId != report.UserId);
        int totalConfirms = allVotes.Count(v => v.Confirms);
        int rejections = allVotes.Count(v => !v.Confirms);

        // Base: 40 pts (o 50 si adjuntó evidencia fotográfica)
        int baseScore = string.IsNullOrWhiteSpace(report.PhotoUrl) ? 40 : 50;
        int computedScore = baseScore + (independentConfirms * 15) - (rejections * 20);
        short finalScore = (short)Math.Clamp(computedScore, 0, 100);

        report.UpdateConfidenceScore(finalScore);

        // Si los rechazos superan a las confirmaciones -> DISPUTED (under_review)
        if (rejections > independentConfirms)
        {
            if (report.Status != ReportStatus.Validated)
            {
                report.SetStatus(ReportStatus.UnderReview);
            }
        }
        else if (independentConfirms >= 2 && finalScore >= 70)
        {
            // Reporte confirmado comunitariamente
            if (report.Status == ReportStatus.Pending || report.Status == ReportStatus.UnderReview)
            {
                report.SetStatus(ReportStatus.Validated);
            }
        }

        // Excepción E4: si el reporte estaba expirado y recibe confirmación, se reactiva
        if (request.Confirms && report.ExpiresAt.HasValue && report.ExpiresAt.Value <= DateTime.UtcNow)
        {
            report.Reactivate(DateTime.UtcNow.AddHours(24));
        }

        _reportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string message = request.Confirms
            ? "¡Gracias por confirmar la presencia del obstáculo!"
            : "¡Gracias por reportar que el obstáculo ya no está!";

        return new VoteObstacleReportResponse(
            report.Id,
            report.ConfidenceScore,
            totalConfirms,
            rejections,
            report.Status.ToPgName(),
            request.Confirms,
            message);
    }

    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6371000;
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
