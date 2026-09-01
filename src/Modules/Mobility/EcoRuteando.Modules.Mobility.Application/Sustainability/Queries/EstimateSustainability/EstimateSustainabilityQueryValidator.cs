using EcoRuteando.Modules.Mobility.Domain.Enums;
using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.Sustainability.Queries.EstimateSustainability;

public sealed class EstimateSustainabilityQueryValidator
    : AbstractValidator<EstimateSustainabilityQuery>
{
    private static readonly string[] ValidTransportTypes =
        ["bike", "public_transport", "mixed", "walking"];

    public EstimateSustainabilityQueryValidator()
    {
        RuleFor(x => x.OriginLat)
            .InclusiveBetween(-90d, 90d)
            .WithMessage("La latitud de origen debe estar entre -90 y 90.");

        RuleFor(x => x.OriginLng)
            .InclusiveBetween(-180d, 180d)
            .WithMessage("La longitud de origen debe estar entre -180 y 180.");

        RuleFor(x => x.DestinationLat)
            .InclusiveBetween(-90d, 90d)
            .WithMessage("La latitud de destino debe estar entre -90 y 90.");

        RuleFor(x => x.DestinationLng)
            .InclusiveBetween(-180d, 180d)
            .WithMessage("La longitud de destino debe estar entre -180 y 180.");

        RuleFor(x => x.TransportMode)
            .NotEmpty()
            .WithMessage("El modo de transporte es obligatorio.")
            .Must(t => ValidTransportTypes.Contains(t))
            .WithMessage($"Tipo de transporte no válido. Valores permitidos: {string.Join(", ", ValidTransportTypes)}.");
    }
}
