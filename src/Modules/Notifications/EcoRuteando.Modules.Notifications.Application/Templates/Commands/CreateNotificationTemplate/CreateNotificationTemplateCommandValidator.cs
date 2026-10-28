using FluentValidation;

namespace EcoRuteando.Modules.Notifications.Application.Templates.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandValidator : AbstractValidator<CreateNotificationTemplateCommand>
{
    public CreateNotificationTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la plantilla es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la plantilla no puede exceder 100 caracteres.");

        RuleFor(x => x.NotificationType)
            .IsInEnum().WithMessage("El tipo de notificación no es válido.");

        RuleFor(x => x.Subject)
            .MaximumLength(200).WithMessage("El asunto no puede exceder 200 caracteres.");

        RuleFor(x => x.HtmlBody)
            .MaximumLength(10000).WithMessage("El cuerpo HTML no puede exceder 10000 caracteres.");

        RuleFor(x => x.TextBody)
            .MaximumLength(4000).WithMessage("El cuerpo de texto no puede exceder 4000 caracteres.");
    }
}
