using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.Ratings.Commands.RateRoute;

public sealed class RateRouteCommandValidator
    : AbstractValidator<RateRouteCommand>
{
    public RateRouteCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .WithMessage("La ruta es obligatoria.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.RatingValue)
            .InclusiveBetween((short)1, (short)5)
            .WithMessage("La calificación debe estar entre 1 y 5 estrellas.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("El comentario no puede superar 1000 caracteres.");
    }
}