using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EcoRuteando.IntegrationTests.Persistence;

public sealed class MobilityModelConfigurationTests
{
    private MobilityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MobilityDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=dummy;Username=dummy;Password=dummy",
                o => o.UseNetTopologySuite())
            .Options;

        return new MobilityDbContext(options);
    }

    [Fact]
    public void FavoriteRouteModel_ShouldMapToMobilitySchemaAndCompositePrimaryKey()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(FavoriteRoute));

        Assert.NotNull(entityType);
        Assert.Equal("favorite_routes", entityType.GetTableName());
        Assert.Equal("mobility", entityType.GetSchema());

        var key = entityType.FindPrimaryKey();
        Assert.NotNull(key);
        var keyProps = key.Properties.Select(p => p.Name).ToList();
        Assert.Contains(nameof(FavoriteRoute.UserId), keyProps);
        Assert.Contains(nameof(FavoriteRoute.RouteId), keyProps);
    }

    [Fact]
    public void RouteUsageModel_ShouldMapToRouteUsagesTable()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(RouteUsage));

        Assert.NotNull(entityType);
        Assert.Equal("route_usage", entityType.GetTableName());
        Assert.Equal("mobility", entityType.GetSchema());
    }

    [Fact]
    public void RatingModel_ShouldMapToRatingsTable()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Rating));

        Assert.NotNull(entityType);
        Assert.Equal("ratings", entityType.GetTableName());
        Assert.Equal("mobility", entityType.GetSchema());
    }

    [Fact]
    public void ObstacleReportModel_ShouldMapToCommunitySchema()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(ObstacleReport));

        Assert.NotNull(entityType);
        Assert.Equal("obstacle_reports", entityType.GetTableName());
        Assert.Equal("community", entityType.GetSchema());
    }
}
