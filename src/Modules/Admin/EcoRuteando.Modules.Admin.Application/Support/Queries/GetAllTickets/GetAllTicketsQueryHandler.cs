using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetAllTickets;

public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, IReadOnlyList<SupportTicketDto>>
{
    private readonly ISupportTicketRepository _repository;

    public GetAllTicketsQueryHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SupportTicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _repository.GetAllAsync(request.Status, request.Priority, cancellationToken);
        return tickets.Select(SupportTicketDto.FromEntity).ToList();
    }
}
