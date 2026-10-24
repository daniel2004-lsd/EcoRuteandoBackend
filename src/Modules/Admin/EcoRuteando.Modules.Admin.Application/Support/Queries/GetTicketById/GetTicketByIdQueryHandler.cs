using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Users;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetTicketById;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, SupportTicketDto?>
{
    private readonly ISupportTicketRepository _repository;
    private readonly IUserLookupService _userLookupService;

    public GetTicketByIdQueryHandler(
        ISupportTicketRepository repository,
        IUserLookupService userLookupService)
    {
        _repository = repository;
        _userLookupService = userLookupService;
    }

    public async Task<SupportTicketDto?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, includeMessages: true, cancellationToken);
        if (ticket == null)
        {
            return null;
        }

        string? email = null;
        string? name = null;
        if (ticket.UserId.HasValue)
        {
            var userDetails = await _userLookupService.GetUsersByIdsAsync(new[] { ticket.UserId.Value }, cancellationToken);
            if (userDetails.TryGetValue(ticket.UserId.Value, out var u))
            {
                email = u.Email;
                name = u.Name;
            }
        }

        return SupportTicketDto.FromEntity(ticket, email, name);
    }
}
