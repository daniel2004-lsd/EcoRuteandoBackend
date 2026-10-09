using NpgsqlTypes;

namespace EcoRuteando.Modules.Mobility.Domain.Enums;

/// <summary>
/// Refleja community.report_status en la base de datos.
/// </summary>
public enum ReportStatus
{
    [PgName("pending")]
    Pending = 0,

    [PgName("under_review")]
    UnderReview = 1,

    [PgName("validated")]
    Validated = 2,

    [PgName("rejected")]
    Rejected = 3
}