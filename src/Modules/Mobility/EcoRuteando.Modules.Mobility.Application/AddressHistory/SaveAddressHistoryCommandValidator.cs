using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.AddressHistory;

public sealed class SaveAddressHistoryCommandValidator
    : AbstractValidator<SaveAddressHistoryCommand>
{
    public SaveAddressHistoryCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.SearchText)
            .NotEmpty()
            .WithMessage("La dirección o texto de búsqueda es obligatorio.")
            .MaximumLength(300)
            .WithMessage("El texto no puede superar 300 caracteres.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m)
            .WithMessage("La latitud es inválida.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m)
            .WithMessage("La longitud es inválida.");
    }
}
