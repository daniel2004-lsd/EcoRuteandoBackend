using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.Exports.Commands.RequestExport;

public sealed class RequestExportCommandValidator : AbstractValidator<RequestExportCommand>
{
    public RequestExportCommandValidator()
    {
        RuleFor(x => x.ExportType)
            .NotEmpty().WithMessage("El tipo de exportación es obligatorio.")
            .MaximumLength(50).WithMessage("El tipo de exportación no puede exceder 50 caracteres.");

        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("El formato de exportación no es válido.");

        RuleFor(x => x.Filters)
            .MaximumLength(1000).WithMessage("Los filtros no pueden exceder 1000 caracteres.");
    }
}
