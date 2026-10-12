using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<TransportFactor> TransportFactors => Set<TransportFactor>();
    public DbSet<DailyStatistic> DailyStatistics => Set<DailyStatistic>();
    public DbSet<AdminExport> Exports => Set<AdminExport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("admin");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
