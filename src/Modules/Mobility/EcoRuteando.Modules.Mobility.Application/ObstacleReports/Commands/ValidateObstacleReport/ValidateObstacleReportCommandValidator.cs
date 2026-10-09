using EcoRuteando.Modules.Mobility.Domain.Enums;
using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.ValidateObstacleReport;

public sealed class ValidateObstacleReportCommandValidator
    : AbstractValidator<ValidateObstacleReportCommand>
{
    private static readonly string[] ValidStatuses =
        { "validated", "rejected" };

    public ValidateObstacleReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty()
            .WithMessage("El reporte es obligatorio.");

        RuleFor(x => x.ValidatorId)
            .NotEmpty()
            .WithMessage("El validador es obligatorio.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("El estado es obligatorio.")
            .Must(s =>
                PgEnumExtensions.TryFromPgName(s, out ReportStatus result)
                && result is ReportStatus.Validated or ReportStatus.Rejected)
            .WithMessage("El reporte solo puede quedar validado o rechazado.");

        RuleFor(x => x.ValidationNote)
            .MaximumLength(1000)
            .WithMessage("La nota de validación no puede superar 1000 caracteres.");
    }
}