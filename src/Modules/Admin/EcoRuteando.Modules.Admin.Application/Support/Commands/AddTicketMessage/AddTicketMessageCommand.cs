using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.AddTicketMessage;

public record AddTicketMessageCommand(
    Guid TicketId,
    Guid? SenderId,
    bool IsAdmin,
    string Message,
    string? AttachmentUrl = null) : IRequest<SupportMessageDto?>;
