using EcoRuteando.Modules.Admin.Application.Support.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Support.Commands.CreateTicket;

public record CreateTicketCommand(
    Guid? UserId,
    string Subject,
    string Message,
    short Priority = 2,
    string? AttachmentUrl = null) : IRequest<SupportTicketDto>;
