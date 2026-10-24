using FluentValidation;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestWeather;

public sealed class IngestWeatherCommandValidator : AbstractValidator<IngestWeatherCommand>
{
    public IngestWeatherCommandValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0).WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0).WithMessage("La longitud debe estar entre -180 y 180.");

        RuleFor(x => x.TemperatureC)
            .InclusiveBetween(-60m, 60m).When(x => x.TemperatureC.HasValue)
            .WithMessage("La temperatura debe estar en un rango razonable (-60°C a 60°C).");

        RuleFor(x => x.HumidityPct)
            .InclusiveBetween((short)0, (short)100).When(x => x.HumidityPct.HasValue)
            .WithMessage("El porcentaje de humedad debe estar entre 0 y 100.");

        RuleFor(x => x.RainProbability)
            .InclusiveBetween((short)0, (short)100).When(x => x.RainProbability.HasValue)
            .WithMessage("La probabilidad de lluvia debe estar entre 0 y 100.");

        RuleFor(x => x.Condition)
            .MaximumLength(100).WithMessage("La condición no puede exceder 100 caracteres.");

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("La fuente no puede exceder 100 caracteres.");

        RuleFor(x => x.ValidUntil)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .WithMessage("La fecha fin de validez debe ser posterior o igual a la fecha de inicio.");
    }
}
