using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.CreateTicket;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, SupportTicketDto>
{
    private readonly ISupportTicketRepository _repository;

    public CreateTicketCommandHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<SupportTicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = SupportTicket.Create(request.UserId, request.Subject, request.Priority);
        ticket.AddMessage(request.UserId, isAdmin: false, request.Message, request.AttachmentUrl);

        await _repository.AddAsync(ticket, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return SupportTicketDto.FromEntity(ticket);
    }
}
