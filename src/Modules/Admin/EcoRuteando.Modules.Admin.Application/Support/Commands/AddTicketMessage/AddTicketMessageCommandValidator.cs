using FluentValidation;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.AddTicketMessage;

public sealed class AddTicketMessageCommandValidator : AbstractValidator<AddTicketMessageCommand>
{
    public AddTicketMessageCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("El identificador del ticket es obligatorio.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("El mensaje es obligatorio.")
            .MaximumLength(4000).WithMessage("El mensaje no puede exceder 4000 caracteres.");

        RuleFor(x => x.AttachmentUrl)
            .MaximumLength(500).WithMessage("La URL del adjunto no puede exceder 500 caracteres.");
    }
}
