using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Domain.Entities;

public class TransportFactor
{
    public Guid Id { get; private set; }
    public TransportType TransportType { get; private set; }
    public decimal Co2FactorKgKm { get; private set; }
    public decimal? CalorieFactorKm { get; private set; }
    public DateTime ValidFrom { get; private set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; private set; }
    public string? Source { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private TransportFactor() { }

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
