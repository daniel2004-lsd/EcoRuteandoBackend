using FluentValidation;

namespace EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestAirQuality;

public sealed class IngestAirQualityCommandValidator : AbstractValidator<IngestAirQualityCommand>
{
    public IngestAirQualityCommandValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0).WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0).WithMessage("La longitud debe estar entre -180 y 180.");

        RuleFor(x => x.Aqi)
            .InclusiveBetween(0, 500).When(x => x.Aqi.HasValue)
            .WithMessage("El índice de calidad del aire (AQI) debe estar entre 0 y 500.");

        RuleFor(x => x.Pm25)
            .GreaterThanOrEqualTo(0).When(x => x.Pm25.HasValue)
            .WithMessage("PM2.5 no puede ser negativo.");

        RuleFor(x => x.Pm10)
            .GreaterThanOrEqualTo(0).When(x => x.Pm10.HasValue)
            .WithMessage("PM10 no puede ser negativo.");

        RuleFor(x => x.Co2Ppm)
            .GreaterThanOrEqualTo(0).When(x => x.Co2Ppm.HasValue)
            .WithMessage("CO2 PPM no puede ser negativo.");

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("La fuente no puede exceder 100 caracteres.");
    }
}
