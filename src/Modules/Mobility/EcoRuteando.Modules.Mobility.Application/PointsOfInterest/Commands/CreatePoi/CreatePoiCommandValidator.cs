using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.CreatePoi;

public sealed class CreatePoiCommandValidator
    : AbstractValidator<CreatePoiCommand>
{
    public CreatePoiCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del punto de interés es obligatorio.")
            .MaximumLength(150)
            .WithMessage("El nombre no puede superar 150 caracteres.");

        RuleFor(x => x.PoiType)
            .NotEmpty()
            .WithMessage("El tipo de punto de interés es obligatorio.")
            .MaximumLength(80)
            .WithMessage("El tipo no puede superar 80 caracteres.");

        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90)
            .WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180)
            .WithMessage("La longitud debe estar entre -180 y 180.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede superar 1000 caracteres.");

        RuleFor(x => x.Address)
            .MaximumLength(255)
            .WithMessage("La dirección no puede superar 255 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.IconUrl)
            .MaximumLength(500)
            .WithMessage("La URL del icono no puede superar 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));

        RuleFor(x => x.Source)
            .MaximumLength(100)
            .WithMessage("El origen no puede superar 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Source));
    }
}