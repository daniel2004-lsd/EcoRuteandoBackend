using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;

public sealed class CreateObstacleReportCommandValidator
    : AbstractValidator<CreateObstacleReportCommand>
{
    public CreateObstacleReportCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.ReportType)
            .NotEmpty()
            .WithMessage("El tipo de obstáculo es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El tipo no puede superar 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La descripción es obligatoria.")
            .MaximumLength(2000)
            .WithMessage("La descripción no puede superar 2000 caracteres.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0)
            .WithMessage("La latitud es inválida.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0)
            .WithMessage("La longitud es inválida.");

        RuleFor(x => x.AddressText)
            .MaximumLength(300)
            .WithMessage("La dirección no puede superar 300 caracteres.");
    }
}