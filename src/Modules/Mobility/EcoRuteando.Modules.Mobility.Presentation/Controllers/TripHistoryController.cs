using EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.CompleteTrip;
using EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.StartTrip;
using EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripById;
using EcoRuteando.Modules.Mobility.Application.TripHistory.Queries.GetTripHistory;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public sealed class TripHistoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public TripHistoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetTripHistory(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var trips = await _mediator.Send(
            new GetTripHistoryQuery(userId),
            cancellationToken);

        return Ok(trips);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetTripById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var trip = await _mediator.Send(
            new GetTripByIdQuery(id, userId),
            cancellationToken);

        return Ok(trip);
    }

    [HttpPost]
    [HasPermission("routes.write")]
    public async Task<IActionResult> StartTrip(
        StartTripCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var commandWithUser = command with { UserId = userId };

        var usageId = await _mediator.Send(
            commandWithUser,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetTripById),
            new { id = usageId },
            new { id = usageId });
    }

    [HttpPost("{id:guid}/complete")]
    [HasPermission("routes.write")]
    public async Task<IActionResult> CompleteTrip(
        Guid id,
        CompleteTripCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.UsageId)
        {
            return BadRequest(new
            {
                message = "El ID de la URL no coincide con el del cuerpo."
            });
        }

        var userId = GetRequiredUserId();

        var commandWithUser = command with { UserId = userId };

        await _mediator.Send(commandWithUser, cancellationToken);

        return NoContent();
    }

    private Guid GetRequiredUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim is null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new Shared.Exceptions.UnauthorizedException(
                "No se pudo identificar al usuario autenticado.");
        }

        return userId;
    }
}
