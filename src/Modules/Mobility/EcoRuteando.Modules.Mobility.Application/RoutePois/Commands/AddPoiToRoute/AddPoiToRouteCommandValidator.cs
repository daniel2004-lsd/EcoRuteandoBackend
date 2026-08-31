using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.AddPoiToRoute;

public sealed class AddPoiToRouteCommandValidator
    : AbstractValidator<AddPoiToRouteCommand>
{
    public AddPoiToRouteCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEqual(Guid.Empty)
            .WithMessage("La ruta es obligatoria.");

        RuleFor(x => x.PoiId)
            .NotEqual(Guid.Empty)
            .WithMessage("El punto de interés es obligatorio.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo((short)0)
            .WithMessage("El orden de visita no puede ser negativo.")
            .When(x => x.SortOrder.HasValue);
    }
}