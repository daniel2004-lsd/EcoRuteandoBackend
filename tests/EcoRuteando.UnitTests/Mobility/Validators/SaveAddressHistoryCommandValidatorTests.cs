using EcoRuteando.Modules.Mobility.Application.AddressHistory;
using FluentValidation.TestHelper;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Validators;

public sealed class SaveAddressHistoryCommandValidatorTests
{
    private readonly SaveAddressHistoryCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        var command = new SaveAddressHistoryCommand(
            Guid.NewGuid(),
            "Carrera 5 # 10-20, Neiva, Huila",
            2.9273m,
            -75.2819m,
            "ChIJN1t_tDeuEmsRUsoyG83frY4",
            "search");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new SaveAddressHistoryCommand(
            Guid.Empty,
            "Carrera 5 # 10-20",
            2.9273m,
            -75.2819m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("El usuario es obligatorio.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenSearchTextIsEmpty_ShouldHaveValidationError(string emptyText)
    {
        var command = new SaveAddressHistoryCommand(
            Guid.NewGuid(),
            emptyText,
            2.9273m,
            -75.2819m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SearchText)
            .WithErrorMessage("La dirección o texto de búsqueda es obligatorio.");
    }

    [Fact]
    public void Validate_WhenCoordinatesAreOutOfRange_ShouldHaveValidationError()
    {
        var command = new SaveAddressHistoryCommand(
            Guid.NewGuid(),
            "Dirección inválida",
            95m,
            -190m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Latitude);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }
}
