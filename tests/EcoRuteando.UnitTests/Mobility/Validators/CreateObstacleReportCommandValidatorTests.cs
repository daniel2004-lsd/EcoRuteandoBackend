using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;
using FluentValidation.TestHelper;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Validators;

public sealed class CreateObstacleReportCommandValidatorTests
{
    private readonly CreateObstacleReportCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValidWithoutPhoto_ShouldNotHaveValidationError()
    {
        var command = new CreateObstacleReportCommand(
            "obstruction",
            "Vía cerrada por mantenimiento",
            2.9273,
            -75.2819,
            "Cra 5 # 10-20",
            Guid.NewGuid(),
            null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenCommandHasValidPhotoUrl_ShouldNotHaveValidationError()
    {
        var command = new CreateObstacleReportCommand(
            "pothole",
            "Hueco profundo en carril derecho",
            2.9273,
            -75.2819,
            "Cra 5 # 10-20",
            Guid.NewGuid(),
            "https://cdn.ecoruteando.org/reports/photo-123.jpg");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenPhotoUrlExceeds500Chars_ShouldHaveValidationError()
    {
        var longUrl = "https://cdn.ecoruteando.org/" + new string('a', 500) + ".jpg";
        var command = new CreateObstacleReportCommand(
            "pothole",
            "Hueco profundo",
            2.9273,
            -75.2819,
            null,
            Guid.NewGuid(),
            longUrl);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhotoUrl)
            .WithErrorMessage("La URL de la fotografía no puede superar 500 caracteres.");
    }

    [Theory]
    [InlineData("not-a-valid-url")]
    [InlineData("ftp://invalidscheme.com/photo.png")]
    [InlineData("file:///etc/passwd")]
    public void Validate_WhenPhotoUrlIsNotValidHttpOrHttps_ShouldHaveValidationError(string invalidUrl)
    {
        var command = new CreateObstacleReportCommand(
            "pothole",
            "Hueco profundo",
            2.9273,
            -75.2819,
            null,
            Guid.NewGuid(),
            invalidUrl);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhotoUrl)
            .WithErrorMessage("La URL de la fotografía debe ser una URL HTTP o HTTPS válida.");
    }
}
