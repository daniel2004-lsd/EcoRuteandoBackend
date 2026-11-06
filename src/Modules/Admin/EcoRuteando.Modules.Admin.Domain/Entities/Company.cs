namespace EcoRuteando.Modules.Admin.Domain.Entities;

public sealed class Company
{
    public Guid Id { get; private set; }
    public string Nit { get; private set; } = null!;
    public string BusinessName { get; private set; } = null!;
    public string Sector { get; private set; } = "passenger_transport";
    public decimal MonthlyCo2BudgetKg { get; private set; } = 50000m;
    public string? ContactEmail { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<CompanyFleetType> _fleetTypes = new();
    public IReadOnlyCollection<CompanyFleetType> FleetTypes => _fleetTypes.AsReadOnly();

    private Company() { }

    public static Company Create(
        string nit,
        string businessName,
        string sector,
        decimal monthlyCo2BudgetKg,
        string? contactEmail)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Nit = nit.Trim(),
            BusinessName = businessName.Trim(),
            Sector = sector.Trim(),
            MonthlyCo2BudgetKg = monthlyCo2BudgetKg > 0 ? monthlyCo2BudgetKg : 50000m,
            ContactEmail = contactEmail?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateBudget(decimal newBudgetKg)
    {
        if (newBudgetKg <= 0)
            throw new ArgumentException("El presupuesto de CO2 debe ser positivo.");

        MonthlyCo2BudgetKg = newBudgetKg;
        UpdatedAt = DateTime.UtcNow;
    }
}
