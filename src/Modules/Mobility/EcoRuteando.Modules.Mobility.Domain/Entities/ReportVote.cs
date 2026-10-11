namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Voto o confirmación ciudadana sobre un reporte de obstáculo (CU22).
/// Tabla community.report_votes con clave primaria compuesta (user_id, report_id).
/// </summary>
public sealed class ReportVote
{
    public Guid UserId { get; private set; }

    public Guid ReportId { get; private set; }

    public bool Confirms { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private ReportVote()
    {
    }

    public ReportVote(Guid userId, Guid reportId, bool confirms)
    {
        UserId = userId;
        ReportId = reportId;
        Confirms = confirms;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateVote(bool confirms)
    {
        Confirms = confirms;
        CreatedAt = DateTime.UtcNow;
    }
}
