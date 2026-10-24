using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.CreateTicket;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("El asunto del ticket es obligatorio.")
            .MaximumLength(200).WithMessage("El asunto no puede exceder 200 caracteres.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("El mensaje del ticket es obligatorio.")
            .MaximumLength(4000).WithMessage("El mensaje no puede exceder 4000 caracteres.");

        RuleFor(x => x.Priority)
            .InclusiveBetween((short)1, (short)4)
            .WithMessage("La prioridad debe estar entre 1 (Baja) y 4 (Urgente).");

        RuleFor(x => x.AttachmentUrl)
            .MaximumLength(500).WithMessage("La URL del adjunto no puede exceder 500 caracteres.");
    }
}
