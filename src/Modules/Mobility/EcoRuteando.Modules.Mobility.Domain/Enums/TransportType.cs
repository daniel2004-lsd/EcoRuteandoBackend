using NpgsqlTypes;

namespace EcoRuteando.Modules.Mobility.Domain.Enums;

/// <summary>
/// Refleja mobility.transport_type en la base de datos.
/// </summary>
public enum TransportType
{
    [PgName("bike")]
    Bike = 0,

    [PgName("public_transport")]
    PublicTransport = 1,

    [PgName("mixed")]
    Mixed = 2,

    [PgName("walking")]
    Walking = 3
}
