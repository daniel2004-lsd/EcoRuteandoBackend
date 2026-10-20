using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetTicketById;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, SupportTicketDto?>
{
    private readonly ISupportTicketRepository _repository;

    public GetTicketByIdQueryHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<SupportTicketDto?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, includeMessages: true, cancellationToken);
        if (ticket == null)
        {
            return null;
        }

        // Si no es admin y no es el dueño del ticket, no tiene acceso
        if (!request.IsAdmin && ticket.UserId != request.RequestingUserId)
        {
            return null;
        }

        string? email = null;
        string? name = null;
        if (ticket.UserId.HasValue)
        {
            var userDetails = await _repository.GetUserDetailsAsync(new[] { ticket.UserId.Value }, cancellationToken);
            if (userDetails.TryGetValue(ticket.UserId.Value, out var u))
            {
                email = u.Email;
                name = u.Name;
            }
        }

        return SupportTicketDto.FromEntity(ticket, email, name);
    }
}
