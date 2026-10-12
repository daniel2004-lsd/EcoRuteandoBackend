using System.Security.Claims;
using EcoRuteando.Modules.Admin.Application.Support.Commands.AddTicketMessage;
using EcoRuteando.Modules.Admin.Application.Support.Commands.CreateTicket;
using EcoRuteando.Modules.Admin.Application.Support.Commands.UpdateTicketStatus;
using EcoRuteando.Modules.Admin.Application.Support.Queries.GetAllTickets;
using EcoRuteando.Modules.Admin.Application.Support.Queries.GetMyTickets;
using EcoRuteando.Modules.Admin.Application.Support.Queries.GetTicketById;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Admin.Presentation.Controllers;

[ApiController]
[Route("api/support/tickets")]
[Authorize]
public class SupportTicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupportTicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea un nuevo ticket de soporte / PQRS ciudadano.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTicket(
        [FromBody] CreateTicketApiRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var ticket = await _mediator.Send(
            new CreateTicketCommand(userId, request.Subject, request.Message, request.Priority, request.AttachmentUrl),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    /// <summary>
    /// Lista los tickets creados por el usuario autenticado.
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyTickets(CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var tickets = await _mediator.Send(new GetMyTicketsQuery(userId), cancellationToken);
        return Ok(tickets);
    }

    /// <summary>
    /// Lista todos los tickets de soporte del sistema (solo administradores).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAllTickets(
        [FromQuery] SupportTicketStatus? status,
        [FromQuery] short? priority,
        CancellationToken cancellationToken)
    {
        var tickets = await _mediator.Send(new GetAllTicketsQuery(status, priority), cancellationToken);
        return Ok(tickets);
    }

    /// <summary>
    /// Obtiene el detalle de un ticket con su hilo de mensajes.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var ticket = await _mediator.Send(new GetTicketByIdQuery(id, userId, isAdmin), cancellationToken);
        if (ticket == null)
        {
            return NotFound(new { message = "Ticket no encontrado o no tiene permisos para verlo." });
        }

        return Ok(ticket);
    }

    /// <summary>
    /// Envía un mensaje en el hilo del ticket (ciudadano o administrador).
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> AddMessage(
        Guid id,
        [FromBody] AddMessageApiRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var message = await _mediator.Send(
            new AddTicketMessageCommand(id, userId, isAdmin, request.Message, request.AttachmentUrl),
            cancellationToken);

        if (message == null)
        {
            return NotFound(new { message = "Ticket no encontrado." });
        }

        return Ok(message);
    }

    /// <summary>
    /// Actualiza el estado del ticket y/o asigna administrador (solo administradores).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusApiRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetRequiredUserId();

        var success = await _mediator.Send(
            new UpdateTicketStatusCommand(id, request.Status, request.AssignToMe ? adminId : null),
            cancellationToken);

        if (!success)
        {
            return NotFound(new { message = "Ticket no encontrado." });
        }

        return NoContent();
    }

    private Guid GetRequiredUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado.");
        }
        return userId;
    }
}

public sealed class CreateTicketApiRequest
{
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public short Priority { get; init; } = 2;
    public string? AttachmentUrl { get; init; }
}

public sealed class AddMessageApiRequest
{
    public string Message { get; init; } = string.Empty;
    public string? AttachmentUrl { get; init; }
}

public sealed class UpdateStatusApiRequest
{
    public SupportTicketStatus Status { get; init; }
    public bool AssignToMe { get; init; } = false;
}
