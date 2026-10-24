using EcoRuteando.Modules.Mobility.Domain.Entities;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Entities;

public sealed class FavoriteRouteTests
{
    [Fact]
    public void Constructor_WithValidData_TrimsLabelAndSetsCreatedAt()
    {
        var userId = Guid.NewGuid();
        var routeId = Guid.NewGuid();

        var favorite = new FavoriteRoute(userId, routeId, "  Ruta al trabajo  ");

        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(routeId, favorite.RouteId);
        Assert.Equal("Ruta al trabajo", favorite.Label);
        Assert.True(favorite.CreatedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceLabel_SetsLabelToNull(string? label)
    {
        var userId = Guid.NewGuid();
        var routeId = Guid.NewGuid();

        var favorite = new FavoriteRoute(userId, routeId, label);

        Assert.Null(favorite.Label);
    }
}
