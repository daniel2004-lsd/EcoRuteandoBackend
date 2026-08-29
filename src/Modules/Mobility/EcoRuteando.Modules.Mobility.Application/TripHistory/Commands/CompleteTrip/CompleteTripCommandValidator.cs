using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.CompleteTrip;

public sealed class CompleteTripCommandValidator
    : AbstractValidator<CompleteTripCommand>
{
    public CompleteTripCommandValidator()
    {
        RuleFor(x => x.UsageId)
            .NotEmpty()
            .WithMessage("El trayecto es obligatorio.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.ActualDistanceKm)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La distancia real no puede ser negativa.")
            .When(x => x.ActualDistanceKm.HasValue);

        RuleFor(x => x.ActualDurationMin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La duración real no puede ser negativa.")
            .When(x => x.ActualDurationMin.HasValue);

        RuleFor(x => x.ActualCo2Kg)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El CO₂ ahorrado no puede ser negativo.")
            .When(x => x.ActualCo2Kg.HasValue);
    }
}
