using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeatherCurrent;

public sealed class GetWeatherCurrentQueryValidator : AbstractValidator<GetWeatherCurrentQuery>
{
    public GetWeatherCurrentQueryValidator()
    {
        RuleFor(x => x.Lat)
            .InclusiveBetween(-90d, 90d)
            .WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180d, 180d)
            .WithMessage("La longitud debe estar entre -180 y 180.");
    }
}