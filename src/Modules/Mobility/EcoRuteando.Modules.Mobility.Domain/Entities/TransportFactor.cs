using EcoRuteando.Modules.Mobility.Domain.Enums;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Factor de conversión por modo de transporte (tabla admin.transport_factors).
/// Se usa en sola lectura para estimar CO₂ ahorrado y calorías a partir de la
/// distancia real obtenida de Google Maps.
/// </summary>
public sealed class TransportFactor
{
    public Guid Id { get; private set; }

    public TransportType TransportType { get; private set; }

    public decimal Co2FactorKgKm { get; private set; }

    public decimal? CalorieFactorKm { get; private set; }

    public DateTime ValidFrom { get; private set; }

    public DateTime? ValidUntil { get; private set; }

    private TransportFactor()
    {
    }
}
