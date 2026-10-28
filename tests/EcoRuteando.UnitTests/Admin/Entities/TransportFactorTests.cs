using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using Xunit;

namespace EcoRuteando.UnitTests.Admin.Entities;

public sealed class TransportFactorTests
{
    [Fact]
    public void Create_WhenValidUntilIsBeforeOrEqualToValidFrom_ThrowsArgumentException()
    {
        var validFrom = DateTime.UtcNow;
        var validUntil = validFrom.AddHours(-1);

        var ex = Assert.Throws<ArgumentException>(() =>
            TransportFactor.Create(TransportType.Bike, 0.0m, 25.0m, validFrom, validUntil));

        Assert.Equal("ValidUntil must be greater than ValidFrom.", ex.Message);
    }

    [Fact]
    public void Create_WithValidParameters_InitializesCorrectly()
    {
        var validFrom = DateTime.UtcNow;
        var validUntil = validFrom.AddDays(365);

        var factor = TransportFactor.Create(
            TransportType.Bike,
            co2FactorKgKm: 0.0m,
            calorieFactorKm: 25.0m,
            validFrom: validFrom,
            validUntil: validUntil,
            source: "  IPCC 2023  ");

        Assert.NotEqual(Guid.Empty, factor.Id);
        Assert.Equal(TransportType.Bike, factor.TransportType);
        Assert.Equal(0.0m, factor.Co2FactorKgKm);
        Assert.Equal(25.0m, factor.CalorieFactorKm);
        Assert.Equal("IPCC 2023", factor.Source);
        Assert.Equal(validFrom, factor.ValidFrom);
        Assert.Equal(validUntil, factor.ValidUntil);
    }

    [Fact]
    public void Update_WhenValidUntilIsInvalid_ThrowsArgumentException()
    {
        var validFrom = DateTime.UtcNow;
        var factor = TransportFactor.Create(TransportType.Walking, 0.0m, 40.0m, validFrom);

        var ex = Assert.Throws<ArgumentException>(() =>
            factor.Update(0.0m, 45.0m, validFrom, validFrom.AddMinutes(-5), "Updated Source"));

        Assert.Equal("ValidUntil must be greater than ValidFrom.", ex.Message);
    }

    [Fact]
    public void Update_WithValidData_UpdatesProperties()
    {
        var validFrom = DateTime.UtcNow;
        var factor = TransportFactor.Create(TransportType.Car, 0.15m, null, validFrom);

        factor.Update(0.12m, null, validFrom, validFrom.AddYears(1), "  New Study  ");

        Assert.Equal(0.12m, factor.Co2FactorKgKm);
        Assert.Null(factor.CalorieFactorKm);
        Assert.Equal("New Study", factor.Source);
    }

    [Fact]
    public void Expire_WhenExpirationBeforeValidFrom_ThrowsArgumentException()
    {
        var validFrom = DateTime.UtcNow;
        var factor = TransportFactor.Create(TransportType.PublicTransport, 0.08m, null, validFrom);

        var ex = Assert.Throws<ArgumentException>(() =>
            factor.Expire(validFrom.AddSeconds(-1)));

        Assert.Equal("Expiration date must be after ValidFrom.", ex.Message);
    }

    [Fact]
    public void Expire_WithValidDate_SetsValidUntil()
    {
        var validFrom = DateTime.UtcNow;
        var factor = TransportFactor.Create(TransportType.PublicTransport, 0.08m, null, validFrom);
        var expiration = validFrom.AddDays(30);

        factor.Expire(expiration);

        Assert.Equal(expiration, factor.ValidUntil);
    }
}
