using EcoRuteando.Modules.Admin.Application.Analytics.Commands.UpsertDailyStatistic;
using Xunit;

namespace EcoRuteando.UnitTests.Admin.Analytics;

public class UpsertDailyStatisticCommandValidatorTests
{
    private readonly UpsertDailyStatisticCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var command = new UpsertDailyStatisticCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            TotalUsers: 100,
            NewUsers: 10,
            ActiveUsers: 50,
            TotalRoutes: 200,
            CompletedRoutes: 150,
            TotalCommunities: 5,
            TotalReports: 12,
            TotalCo2Saved: 34.5m,
            TotalDistanceKm: 120.8m);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FutureDate_ShouldFailValidation()
    {
        var command = new UpsertDailyStatisticCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            TotalUsers: 10,
            NewUsers: 1,
            ActiveUsers: 5,
            TotalRoutes: 20,
            CompletedRoutes: 15,
            TotalCommunities: 1,
            TotalReports: 2,
            TotalCo2Saved: 3.5m,
            TotalDistanceKm: 12.0m);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpsertDailyStatisticCommand.StatisticDate));
    }

    [Fact]
    public void NegativeCounters_ShouldFailValidation()
    {
        var command = new UpsertDailyStatisticCommand(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            TotalUsers: -1,
            NewUsers: -5,
            ActiveUsers: 0,
            TotalRoutes: -10,
            CompletedRoutes: 0,
            TotalCommunities: 0,
            TotalReports: 0,
            TotalCo2Saved: -1.0m,
            TotalDistanceKm: -5.0m);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5);
    }
}
