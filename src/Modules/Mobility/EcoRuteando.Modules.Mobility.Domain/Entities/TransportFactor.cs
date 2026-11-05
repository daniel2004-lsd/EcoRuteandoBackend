namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Factor de conversión por modo de transporte (tabla admin.transport_factors).
/// Deriva de la definición canónica única de TransportFactor en Admin.Domain (H-18).
/// </summary>
public class TransportFactor : EcoRuteando.Modules.Admin.Domain.Entities.TransportFactor
{
    public TransportFactor() : base()
    {
    }
}
