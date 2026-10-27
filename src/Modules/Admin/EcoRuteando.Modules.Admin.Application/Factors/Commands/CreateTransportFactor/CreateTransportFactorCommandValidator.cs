using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.Factors.Commands.CreateTransportFactor;

public sealed class CreateTransportFactorCommandValidator : AbstractValidator<CreateTransportFactorCommand>
{
    public CreateTransportFactorCommandValidator()
    {
        RuleFor(x => x.TransportType)
            .IsInEnum().WithMessage("El tipo de transporte no es válido.");

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
            .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
            .When(x => x.ValidUntil.HasValue && x.ValidFrom.HasValue)
            .WithMessage("La fecha de fin de vigencia debe ser posterior o igual a la fecha de inicio.");
    }
}
