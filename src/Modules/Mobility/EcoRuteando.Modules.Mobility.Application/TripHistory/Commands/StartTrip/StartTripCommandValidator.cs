using EcoRuteando.Modules.Mobility.Domain.Enums;
using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.StartTrip;

public sealed class StartTripCommandValidator
    : AbstractValidator<StartTripCommand>
{
    private static readonly string[] ValidTransportTypes =
        ["bike", "public_transport", "mixed", "walking"];

    private static readonly string[] ValidSources =
        ["web", "mobile", "pwa"];

    public StartTripCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .WithMessage("La ruta es obligatoria.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.TransportMode)
            .Must(t => ValidTransportTypes.Contains(t!))
            .WithMessage($"Tipo de transporte no válido. Valores permitidos: {string.Join(", ", ValidTransportTypes)}.")
            .When(x => !string.IsNullOrWhiteSpace(x.TransportMode));

        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("El origen es obligatorio.")
            .Must(s => ValidSources.Contains(s))
            .WithMessage($"El origen no es válido. Valores permitidos: {string.Join(", ", ValidSources)}.");
    }
}
