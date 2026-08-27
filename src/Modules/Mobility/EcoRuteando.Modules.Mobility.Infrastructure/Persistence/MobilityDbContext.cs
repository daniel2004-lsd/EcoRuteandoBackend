using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence;

public class MobilityDbContext : DbContext, IMobilityUnitOfWork
{
    public MobilityDbContext(DbContextOptions<MobilityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Route> Routes => Set<Route>();

    public DbSet<PointOfInterest> PointsOfInterest => Set<PointOfInterest>();

    public DbSet<RoutePoi> RoutePois => Set<RoutePoi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum(
            "mobility",
            "transport_type",
            new[] { "bike", "public_transport", "mixed", "walking" });

        modelBuilder.HasPostgresEnum(
            "mobility",
            "route_status",
            new[] { "active", "inactive", "under_review", "archived" });

        modelBuilder.ApplyConfiguration(new RouteConfiguration());
        modelBuilder.ApplyConfiguration(new PointOfInterestConfiguration());
        modelBuilder.ApplyConfiguration(new RoutePoiConfiguration());
    }
}
