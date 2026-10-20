using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetMyTickets;

public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, IReadOnlyList<SupportTicketDto>>
{
    private readonly ISupportTicketRepository _repository;

    public GetMyTicketsQueryHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SupportTicketDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        return tickets.Select(t => SupportTicketDto.FromEntity(t)).ToList();
    }
}
