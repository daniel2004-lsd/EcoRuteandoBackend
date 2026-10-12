using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.UpdateTicketStatus;

public record UpdateTicketStatusCommand(
    Guid TicketId,
    SupportTicketStatus NewStatus,
    Guid? AdminId = null) : IRequest<bool>;
