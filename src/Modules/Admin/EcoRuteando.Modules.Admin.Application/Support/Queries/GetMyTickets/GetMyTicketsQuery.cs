using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetMyTickets;

public record GetMyTicketsQuery(Guid UserId) : IRequest<IReadOnlyList<SupportTicketDto>>;
