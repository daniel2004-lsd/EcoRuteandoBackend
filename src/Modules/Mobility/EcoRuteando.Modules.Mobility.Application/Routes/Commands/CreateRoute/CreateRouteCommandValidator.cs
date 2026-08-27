using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.Routes.Commands.CreateRoute;

public sealed class CreateRouteCommandValidator
    : AbstractValidator<CreateRouteCommand>
{
    private static readonly string[] ValidTransportTypes =
        ["bike", "public_transport", "mixed", "walking"];

    public CreateRouteCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la ruta es obligatorio.")
            .MaximumLength(150)
            .WithMessage("El nombre no puede superar 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede superar 1000 caracteres.");

        RuleFor(x => x.TransportType)
            .NotEmpty()
            .WithMessage("El tipo de transporte es obligatorio.")
            .Must(t => ValidTransportTypes.Contains(t))
            .WithMessage($"Tipo de transporte no válido. Valores permitidos: {string.Join(", ", ValidTransportTypes)}.");

        RuleFor(x => x.StartName)
            .NotEmpty()
            .WithMessage("El punto de origen es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El punto de origen no puede superar 200 caracteres.");

        RuleFor(x => x.DestinationName)
            .NotEmpty()
            .WithMessage("El punto de destino es obligatorio.")
            .MaximumLength(200)
            .WithMessage("El punto de destino no puede superar 200 caracteres.");

        RuleFor(x => x.StartLat)
            .InclusiveBetween(-90, 90)
            .WithMessage("La latitud de origen debe estar entre -90 y 90.")
            .When(x => x.StartLat.HasValue);

        RuleFor(x => x.StartLng)
            .InclusiveBetween(-180, 180)
            .WithMessage("La longitud de origen debe estar entre -180 y 180.")
            .When(x => x.StartLng.HasValue);

        RuleFor(x => x.EndLat)
            .InclusiveBetween(-90, 90)
            .WithMessage("La latitud de destino debe estar entre -90 y 90.")
            .When(x => x.EndLat.HasValue);

        RuleFor(x => x.EndLng)
            .InclusiveBetween(-180, 180)
            .WithMessage("La longitud de destino debe estar entre -180 y 180.")
            .When(x => x.EndLng.HasValue);

        RuleFor(x => x.DistanceKm)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La distancia no puede ser negativa.")
            .When(x => x.DistanceKm.HasValue);

        RuleFor(x => x.EstimatedTimeMin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El tiempo estimado no puede ser negativo.")
            .When(x => x.EstimatedTimeMin.HasValue);

        RuleFor(x => x.DifficultyLevel)
            .InclusiveBetween((short)1, (short)5)
            .WithMessage("El nivel de dificultad debe estar entre 1 y 5.")
            .When(x => x.DifficultyLevel.HasValue);

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500)
            .WithMessage("La URL de la foto no puede superar 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.PhotoUrl));
    }
}
