using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Users;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetAllTickets;

public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, IReadOnlyList<SupportTicketDto>>
{
    private readonly ISupportTicketRepository _repository;
    private readonly IUserLookupService _userLookupService;

    public GetAllTicketsQueryHandler(
        ISupportTicketRepository repository,
        IUserLookupService userLookupService)
    {
        _repository = repository;
        _userLookupService = userLookupService;
    }

    public async Task<IReadOnlyList<SupportTicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _repository.GetAllAsync(request.Status, request.Priority, cancellationToken);

        var userIds = tickets.Where(t => t.UserId.HasValue).Select(t => t.UserId!.Value).Distinct();
        var userDetails = await _userLookupService.GetUsersByIdsAsync(userIds, cancellationToken);

        return tickets.Select(t =>
        {
            string? email = null;
            string? name = null;
            if (t.UserId.HasValue && userDetails.TryGetValue(t.UserId.Value, out var u))
            {
                email = u.Email;
                name = u.Name;
            }
            return SupportTicketDto.FromEntity(t, email, name);
        }).ToList();
    }
}
