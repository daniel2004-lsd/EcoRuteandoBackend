using FluentValidation;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.SendNotification;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título de la notificación es obligatorio.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("El mensaje de la notificación es obligatorio.")
            .MaximumLength(1000).WithMessage("El mensaje no puede exceder 1000 caracteres.");

        RuleFor(x => x.NotificationType)
            .IsInEnum().WithMessage("El tipo de notificación no es válido.");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("El canal de notificación no es válido.");

        RuleFor(x => x.ActionUrl)
            .MaximumLength(500).WithMessage("La URL de acción no puede exceder 500 caracteres.");
    }
}
