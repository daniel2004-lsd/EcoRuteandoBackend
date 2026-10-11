using EcoRuteando.Modules.Mobility.Application.Abstractions;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence;

public class MobilityDbContext : DbContext, IMobilityDbContext
{
    public MobilityDbContext(DbContextOptions<MobilityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Route> Routes => Set<Route>();

    public DbSet<PointOfInterest> PointsOfInterest => Set<PointOfInterest>();

    public DbSet<RoutePoi> RoutePois => Set<RoutePoi>();

    public DbSet<RouteUsage> RouteUsages => Set<RouteUsage>();

    public DbSet<TransportFactor> TransportFactors => Set<TransportFactor>();

    public DbSet<FavoriteRoute> FavoriteRoutes => Set<FavoriteRoute>();

    public DbSet<SharedRoute> SharedRoutes => Set<SharedRoute>();

    public DbSet<Rating> Ratings => Set<Rating>();

    public DbSet<ObstacleReport> ObstacleReports => Set<ObstacleReport>();

    public DbSet<ReportVote> ReportVotes => Set<ReportVote>();

    public DbSet<AddressHistory> AddressHistory => Set<AddressHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum(
            "mobility",
            "transport_type",
            new[] { "bike", "public_transport", "mixed", "walking", "car" });

        modelBuilder.HasPostgresEnum(
            "mobility",
            "route_status",
            new[] { "active", "inactive", "under_review", "archived" });

        modelBuilder.HasPostgresEnum(
            "mobility",
            "usage_source",
            new[] { "web", "mobile", "pwa" });

        modelBuilder.HasPostgresEnum(
            "community",
            "report_status",
            new[] { "pending", "under_review", "validated", "rejected" });

        modelBuilder.ApplyConfiguration(new RouteConfiguration());
        modelBuilder.ApplyConfiguration(new PointOfInterestConfiguration());
        modelBuilder.ApplyConfiguration(new RoutePoiConfiguration());
        modelBuilder.ApplyConfiguration(new RouteUsageConfiguration());
        modelBuilder.ApplyConfiguration(new TransportFactorConfiguration());
        modelBuilder.ApplyConfiguration(new FavoriteRouteConfiguration());
        modelBuilder.ApplyConfiguration(new SharedRouteConfiguration());
        modelBuilder.ApplyConfiguration(new RatingConfiguration());
        modelBuilder.ApplyConfiguration(new ObstacleReportConfiguration());
        modelBuilder.ApplyConfiguration(new ReportVoteConfiguration());
    }
}
