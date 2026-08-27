using NpgsqlTypes;

namespace EcoRuteando.Modules.Mobility.Domain.Enums;

/// <summary>
/// Refleja mobility.route_status en la base de datos.
/// </summary>
public enum RouteStatus
{
    [PgName("active")]
    Active = 0,

    [PgName("inactive")]
    Inactive = 1,

    [PgName("under_review")]
    UnderReview = 2,

    [PgName("archived")]
    Archived = 3
}
