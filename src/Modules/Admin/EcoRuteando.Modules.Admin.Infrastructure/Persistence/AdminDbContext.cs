using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext, IAdminUnitOfWork
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<TransportFactor> TransportFactors => Set<TransportFactor>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyFleetType> CompanyFleetTypes => Set<CompanyFleetType>();
    public DbSet<CarbonCompensation> CarbonCompensations => Set<CarbonCompensation>();
    public DbSet<DailyStatistic> DailyStatistics => Set<DailyStatistic>();
    public DbSet<AdminExport> Exports => Set<AdminExport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("admin");

        modelBuilder.HasPostgresEnum(
            "admin",
            "support_status",
            new[] { "pending", "in_progress", "resolved", "closed" });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
