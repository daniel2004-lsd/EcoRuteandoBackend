using FluentValidation;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.RegisterDeviceToken;

public sealed class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El identificador de usuario es obligatorio.");

        RuleFor(x => x.Platform)
            .NotEmpty().WithMessage("La plataforma es obligatoria.")
            .MaximumLength(50).WithMessage("La plataforma no puede exceder 50 caracteres.");

        RuleFor(x => x.ExpoPushToken)
            .NotEmpty().WithMessage("El token del dispositivo es obligatorio.")
            .MaximumLength(500).WithMessage("El token del dispositivo no puede exceder 500 caracteres.");

        RuleFor(x => x.DeviceId)
            .MaximumLength(200).WithMessage("El ID del dispositivo no puede exceder 200 caracteres.");
    }
}
