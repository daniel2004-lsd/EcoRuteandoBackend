using NpgsqlTypes;

namespace EcoRuteando.Modules.Mobility.Domain.Enums;

/// <summary>
/// Refleja mobility.usage_source en la base de datos.
/// </summary>
public enum UsageSource
{
    [PgName("web")]
    Web = 0,

    [PgName("mobile")]
    Mobile = 1,

    [PgName("pwa")]
    Pwa = 2
}
