using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.RemovePoiFromRoute;

public sealed class RemovePoiFromRouteCommandValidator
    : AbstractValidator<RemovePoiFromRouteCommand>
{
    public RemovePoiFromRouteCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEqual(Guid.Empty)
            .WithMessage("La ruta es obligatoria.");

        RuleFor(x => x.PoiId)
            .NotEqual(Guid.Empty)
            .WithMessage("El punto de interés es obligatorio.");
    }
}