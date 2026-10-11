using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Queries.GetTicketById;

public record GetTicketByIdQuery(Guid TicketId, Guid RequestingUserId, bool IsAdmin) : IRequest<SupportTicketDto?>;
