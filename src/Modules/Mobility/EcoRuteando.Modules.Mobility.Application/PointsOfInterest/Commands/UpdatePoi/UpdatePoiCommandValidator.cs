using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.UpdatePoi;

public sealed class UpdatePoiCommandValidator
    : AbstractValidator<UpdatePoiCommand>
{
    public UpdatePoiCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("El punto de interés es obligatorio.");

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
            .WithMessage("La latitud debe estar entre -90 y 90.")
            .When(x => x.Lat.HasValue);

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180)
            .WithMessage("La longitud debe estar entre -180 y 180.")
            .When(x => x.Lng.HasValue);

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
    }
}