using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetAllTickets;

public record GetAllTicketsQuery(SupportTicketStatus? Status = null, short? Priority = null) : IRequest<IReadOnlyList<SupportTicketDto>>;
