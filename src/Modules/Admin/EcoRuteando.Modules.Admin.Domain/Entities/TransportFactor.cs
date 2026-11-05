using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Domain.Entities;

/// <summary>
/// Factor de conversión por modo de transporte (tabla admin.transport_factors).
/// Definición canónica única en el sistema (H-18).
/// </summary>
public class TransportFactor
{
    public Guid Id { get; protected set; }
    public TransportType TransportType { get; protected set; }
    public decimal Co2FactorKgKm { get; protected set; }
    public decimal? CalorieFactorKm { get; protected set; }
    public DateTime ValidFrom { get; protected set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; protected set; }
    public string? Source { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public TransportFactor() { }

    public static TransportFactor Create(
        TransportType transportType,
        decimal co2FactorKgKm,
        decimal? calorieFactorKm,
        DateTime validFrom,
        DateTime? validUntil = null,
        string? source = null)
    {
        if (validUntil.HasValue && validUntil <= validFrom)
            throw new ArgumentException("ValidUntil must be greater than ValidFrom.");

        return new TransportFactor
        {
            Id = Guid.NewGuid(),
            TransportType = transportType,
            Co2FactorKgKm = co2FactorKgKm,
            CalorieFactorKm = calorieFactorKm,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        decimal co2FactorKgKm,
        decimal? calorieFactorKm,
        DateTime validFrom,
        DateTime? validUntil,
        string? source)
    {
        if (validUntil.HasValue && validUntil <= validFrom)
            throw new ArgumentException("ValidUntil must be greater than ValidFrom.");

        Co2FactorKgKm = co2FactorKgKm;
        CalorieFactorKm = calorieFactorKm;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
    }

    public void Expire(DateTime expirationTime)
    {
        if (expirationTime <= ValidFrom)
            throw new ArgumentException("Expiration date must be after ValidFrom.");

        ValidUntil = expirationTime;
    }
}
