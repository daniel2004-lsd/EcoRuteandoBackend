namespace EcoRuteando.Modules.Admin.Domain.Entities;

public sealed class CompanyFleetType
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string VehicleType { get; private set; } = null!;
    public string FuelType { get; private set; } = null!;
    public decimal EmissionFactorKgKm { get; private set; }
    public string SourceReference { get; private set; } = "UPME / MinAmbiente Colombia";
    public bool IsActive { get; private set; } = true;
    public DateTime ValidFrom { get; private set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private CompanyFleetType() { }

    public static CompanyFleetType Create(
        Guid companyId,
        string vehicleType,
        string fuelType,
        decimal emissionFactorKgKm,
        string sourceReference = "UPME / MinAmbiente Colombia")
    {
        return new CompanyFleetType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            VehicleType = vehicleType.Trim(),
            FuelType = fuelType.Trim(),
            EmissionFactorKgKm = emissionFactorKgKm > 0 ? emissionFactorKgKm : 0.200m,
            SourceReference = sourceReference.Trim(),
            IsActive = true,
            ValidFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
