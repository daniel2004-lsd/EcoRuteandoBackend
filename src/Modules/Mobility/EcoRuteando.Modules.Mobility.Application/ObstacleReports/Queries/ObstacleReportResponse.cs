namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries;

/// <summary>
/// Reporte de obstáculo para respuestas de consulta.
/// El status se expone como label de PostgreSQL (pending/under_review/validated/rejected).
/// </summary>
public sealed record ObstacleReportResponse(
    Guid Id,
    string ReportType,
    string Description,
    double Latitude,
    double Longitude,
    string? AddressText,
    string? PhotoUrl,
    string Status,
    string? ValidationNote,
    DateTime? ValidatedAt,
    DateTime CreatedAt);