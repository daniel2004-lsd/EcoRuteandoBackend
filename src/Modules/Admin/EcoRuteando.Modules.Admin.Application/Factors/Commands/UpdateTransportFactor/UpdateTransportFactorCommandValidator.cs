using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.Factors.Commands.UpdateTransportFactor;

public sealed class UpdateTransportFactorCommandValidator : AbstractValidator<UpdateTransportFactorCommand>
{
    public UpdateTransportFactorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador del factor de transporte es obligatorio.");

        RuleFor(x => x.Co2FactorKgKm)
            .GreaterThanOrEqualTo(0).WithMessage("El factor de CO2 no puede ser negativo.")
            .LessThanOrEqualTo(1000).WithMessage("El factor de CO2 excede el límite permitido.");

        RuleFor(x => x.CalorieFactorKm)
            .GreaterThanOrEqualTo(0).When(x => x.CalorieFactorKm.HasValue)
            .WithMessage("El factor de calorías no puede ser negativo.")
            .LessThanOrEqualTo(5000).When(x => x.CalorieFactorKm.HasValue)
            .WithMessage("El factor de calorías excede el límite permitido.");

        RuleFor(x => x.Source)
            .MaximumLength(255).WithMessage("La fuente no puede exceder 255 caracteres.");

        RuleFor(x => x.ValidUntil)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .When(x => x.ValidUntil.HasValue)
            .WithMessage("La fecha de fin de vigencia debe ser posterior o igual a la fecha de inicio.");
    }
}
