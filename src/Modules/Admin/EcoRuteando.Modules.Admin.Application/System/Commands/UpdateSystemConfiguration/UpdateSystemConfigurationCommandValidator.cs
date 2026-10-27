using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.System.Commands.UpdateSystemConfiguration;

public sealed class UpdateSystemConfigurationCommandValidator : AbstractValidator<UpdateSystemConfigurationCommand>
{
    public UpdateSystemConfigurationCommandValidator()
    {
        RuleFor(x => x.AppName)
            .NotEmpty().WithMessage("El nombre de la aplicación es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la aplicación no puede exceder 100 caracteres.");

        RuleFor(x => x.AdminEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.AdminEmail))
            .WithMessage("El correo electrónico del administrador no es válido.")
            .MaximumLength(255).WithMessage("El correo electrónico no puede exceder 255 caracteres.");

        RuleFor(x => x.SupportPhone)
            .MaximumLength(50).WithMessage("El teléfono de soporte no puede exceder 50 caracteres.");

        RuleFor(x => x.MapCacheTtlSec)
            .InclusiveBetween(0, 604800).WithMessage("El TTL del caché de mapas debe estar entre 0 y 604800 segundos.");

        RuleFor(x => x.WeatherCacheTtlSec)
            .InclusiveBetween(0, 604800).WithMessage("El TTL del caché de clima debe estar entre 0 y 604800 segundos.");

        RuleFor(x => x.PrimaryColor)
            .NotEmpty().WithMessage("El color primario es obligatorio.")
            .MaximumLength(20).WithMessage("El color primario no puede exceder 20 caracteres.");

        RuleFor(x => x.SecondaryColor)
            .NotEmpty().WithMessage("El color secundario es obligatorio.")
            .MaximumLength(20).WithMessage("El color secundario no puede exceder 20 caracteres.");
    }
}
