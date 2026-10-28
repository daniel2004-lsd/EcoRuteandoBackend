using EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.CompleteTrip;
using EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.StartTrip;
using FluentValidation.TestHelper;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Validators;

public sealed class TripValidatorsTests
{
    private readonly StartTripCommandValidator _startValidator = new();
    private readonly CompleteTripCommandValidator _completeValidator = new();

    [Fact]
    public void StartTrip_WhenRouteIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new StartTripCommand(Guid.Empty, "bike", "mobile", Guid.NewGuid(), null);

        var result = _startValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RouteId)
              .WithErrorMessage("La ruta es obligatoria.");
    }

    [Fact]
    public void StartTrip_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new StartTripCommand(Guid.NewGuid(), "bike", "mobile", Guid.Empty, null);

        var result = _startValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("El usuario es obligatorio.");
    }

    [Theory]
    [InlineData("airplane")]
    [InlineData("submarine")]
    [InlineData("rocket")]
    public void StartTrip_WhenTransportModeIsInvalid_ShouldHaveValidationError(string invalidMode)
    {
        var command = new StartTripCommand(Guid.NewGuid(), invalidMode, "mobile", Guid.NewGuid(), null);

        var result = _startValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TransportMode);
    }

    [Theory]
    [InlineData("bike")]
    [InlineData("car")]
    [InlineData("public_transport")]
    [InlineData("mixed")]
    [InlineData("walking")]
    [InlineData(null)]
    public void StartTrip_WhenTransportModeIsValidOrNull_ShouldNotHaveValidationError(string? validMode)
    {
        var command = new StartTripCommand(Guid.NewGuid(), validMode, "mobile", Guid.NewGuid(), null);

        var result = _startValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.TransportMode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("desktop")]
    [InlineData("smartwatch")]
    public void StartTrip_WhenSourceIsInvalid_ShouldHaveValidationError(string invalidSource)
    {
        var command = new StartTripCommand(Guid.NewGuid(), "bike", invalidSource, Guid.NewGuid(), null);

        var result = _startValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Source);
    }

    [Theory]
    [InlineData("web")]
    [InlineData("mobile")]
    [InlineData("pwa")]
    public void StartTrip_WhenSourceIsValid_ShouldNotHaveValidationError(string validSource)
    {
        var command = new StartTripCommand(Guid.NewGuid(), "bike", validSource, Guid.NewGuid(), null);

        var result = _startValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Source);
    }

    [Fact]
    public void CompleteTrip_WhenUsageIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CompleteTripCommand(Guid.Empty, Guid.NewGuid(), 10m, 30, 0.5m, DateTime.UtcNow, null);

        var result = _completeValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UsageId)
              .WithErrorMessage("El trayecto es obligatorio.");
    }

    [Fact]
    public void CompleteTrip_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CompleteTripCommand(Guid.NewGuid(), Guid.Empty, 10m, 30, 0.5m, DateTime.UtcNow, null);

        var result = _completeValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("El usuario es obligatorio.");
    }

    [Fact]
    public void CompleteTrip_WhenActualDistanceKmIsNegative_ShouldHaveValidationError()
    {
        var command = new CompleteTripCommand(Guid.NewGuid(), Guid.NewGuid(), -1m, 30, 0.5m, DateTime.UtcNow, null);

        var result = _completeValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActualDistanceKm)
              .WithErrorMessage("La distancia real no puede ser negativa.");
    }

    [Fact]
    public void CompleteTrip_WhenActualDurationMinIsNegative_ShouldHaveValidationError()
    {
        var command = new CompleteTripCommand(Guid.NewGuid(), Guid.NewGuid(), 10m, -5, 0.5m, DateTime.UtcNow, null);

        var result = _completeValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActualDurationMin)
              .WithErrorMessage("La duración real no puede ser negativa.");
    }

    [Fact]
    public void CompleteTrip_WhenActualCo2KgIsNegative_ShouldHaveValidationError()
    {
        var command = new CompleteTripCommand(Guid.NewGuid(), Guid.NewGuid(), 10m, 30, -0.2m, DateTime.UtcNow, null);

        var result = _completeValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ActualCo2Kg)
              .WithErrorMessage("El CO₂ ahorrado no puede ser negativo.");
    }

    [Fact]
    public void CompleteTrip_WithValidParameters_ShouldPassValidation()
    {
        var command = new CompleteTripCommand(Guid.NewGuid(), Guid.NewGuid(), 5.2m, 25, 0.75m, DateTime.UtcNow, null);

        var result = _completeValidator.TestValidate(command);

        Assert.True(result.IsValid);
    }
}
