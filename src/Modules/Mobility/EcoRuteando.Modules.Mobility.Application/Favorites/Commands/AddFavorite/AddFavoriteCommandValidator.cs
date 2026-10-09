using FluentValidation;

namespace EcoRuteando.Modules.Mobility.Application.Favorites.Commands.AddFavorite;

public sealed class AddFavoriteCommandValidator
    : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.RouteId)
            .NotEmpty()
            .WithMessage("La ruta es obligatoria.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.Label)
            .MaximumLength(80)
            .WithMessage("La etiqueta no puede superar 80 caracteres.");
    }
}
