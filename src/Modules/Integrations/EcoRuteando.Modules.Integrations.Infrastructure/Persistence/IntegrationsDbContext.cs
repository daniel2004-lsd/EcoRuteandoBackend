using EcoRuteando.Modules.Integrations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Integrations.Infrastructure.Persistence;

public class IntegrationsDbContext : DbContext
{
    public IntegrationsDbContext(DbContextOptions<IntegrationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<AirQualityData> AirQualityData => Set<AirQualityData>();
    public DbSet<WeatherData> WeatherData => Set<WeatherData>();
    public DbSet<ApiCacheEntry> ApiCache => Set<ApiCacheEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("integrations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
