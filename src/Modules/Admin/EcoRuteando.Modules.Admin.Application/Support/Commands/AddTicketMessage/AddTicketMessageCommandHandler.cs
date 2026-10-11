using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.AddTicketMessage;

public class AddTicketMessageCommandHandler : IRequestHandler<AddTicketMessageCommand, SupportMessageDto?>
{
    private readonly ISupportTicketRepository _repository;

    public AddTicketMessageCommandHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<SupportMessageDto?> Handle(AddTicketMessageCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, includeMessages: false, cancellationToken);
        if (ticket == null)
        {
            return null;
        }

        if (request.IsAdmin && ticket.Status == SupportTicketStatus.Pending)
        {
            ticket.UpdateStatus(SupportTicketStatus.InProgress);
        }

        var message = SupportMessage.Create(
            ticket.Id,
            request.SenderId,
            request.IsAdmin,
            request.Message,
            request.AttachmentUrl);

        await _repository.AddMessageAsync(message, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return SupportMessageDto.FromEntity(message);
    }
}
