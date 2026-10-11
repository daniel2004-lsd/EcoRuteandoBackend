using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, bool>
{
    private readonly ISupportTicketRepository _repository;

    public UpdateTicketStatusCommandHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, includeMessages: false, cancellationToken);
        if (ticket == null)
        {
            return false;
        }

        if (request.AdminId.HasValue)
        {
            ticket.AssignAdmin(request.AdminId.Value);
        }

        ticket.UpdateStatus(request.NewStatus);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
