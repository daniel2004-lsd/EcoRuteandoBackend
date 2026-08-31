using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.CreatePoi;
using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.DeactivatePoi;
using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.UpdatePoi;
using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPoiById;
using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Queries.GetPois;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/pois")]
[Authorize]
public sealed class PoisController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los puntos de interés activos, opcionalmente filtrados por tipo.
    /// </summary>
    [HttpGet]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetPois(
        [FromQuery] string? poiType,
        CancellationToken cancellationToken)
    {
        var pois = await _mediator.Send(
            new GetPoisQuery(poiType),
            cancellationToken);

        return Ok(pois);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetPoiById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var poi = await _mediator.Send(
            new GetPoiByIdQuery(id),
            cancellationToken);

        return Ok(poi);
    }

    [HttpPost]
    [HasPermission("routes.write")]
    public async Task<IActionResult> CreatePoi(
        CreatePoiCommand command,
        CancellationToken cancellationToken)
    {
        var commandWithUser = command with { CreatedBy = GetUserId() };

        var poiId = await _mediator.Send(
            commandWithUser,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetPoiById),
            new { id = poiId },
            new { id = poiId });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("routes.write")]
    public async Task<IActionResult> UpdatePoi(
        Guid id,
        UpdatePoiCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new
            {
                message = "El ID de la URL no coincide con el del cuerpo."
            });
        }

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("routes.write")]
    public async Task<IActionResult> DeactivatePoi(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeactivatePoiCommand(id),
            cancellationToken);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim is not null && Guid.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}