using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;

public sealed class GetWeatherQueryValidator : AbstractValidator<GetWeatherQuery>
{
    public GetWeatherQueryValidator()
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
    }
}