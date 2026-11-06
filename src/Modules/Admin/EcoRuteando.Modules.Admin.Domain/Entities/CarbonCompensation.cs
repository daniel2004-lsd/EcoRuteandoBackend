namespace EcoRuteando.Modules.Admin.Domain.Entities;

public sealed class CarbonCompensation
{
    public Guid Id { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? UserId { get; private set; }
    public decimal Co2CompensatedKg { get; private set; }
    public int TreesEquivalent { get; private set; }
    public string PartnerProgram { get; private set; } = "Reforestación Cuenca Río Las Ceibas (CAM Huila)";
    public string CertificateCode { get; private set; } = null!;
    public string CompensationStatus { get; private set; } = "certified";
    public string? Notes { get; private set; }
    public DateTime CertifiedAt { get; private set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private CarbonCompensation() { }

    public static CarbonCompensation CreateVoluntary(
        Guid? companyId,
        Guid? userId,
        decimal co2CompensatedKg,
        string partnerProgram = "Reforestación Cuenca Río Las Ceibas (CAM Huila)",
        string? notes = null)
    {
        if (co2CompensatedKg <= 0)
            throw new ArgumentException("El CO2 a compensar debe ser un valor positivo.");

        // Equivalencia biológica científica: 15 kg CO2 al año por cada árbol nativo en crecimiento
        var trees = (int)Math.Ceiling(co2CompensatedKg / 15m);
        var certCode = $"CERT-CAM-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        return new CarbonCompensation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = userId,
            Co2CompensatedKg = co2CompensatedKg,
            TreesEquivalent = trees,
            PartnerProgram = partnerProgram.Trim(),
            CertificateCode = certCode,
            CompensationStatus = "certified",
            Notes = notes?.Trim(),
            CertifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
