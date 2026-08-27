using EcoRuteando.Modules.Mobility.Application.Routes.Commands.CreateRoute;
using EcoRuteando.Modules.Mobility.Application.Routes.Commands.DeleteRoute;
using EcoRuteando.Modules.Mobility.Application.Routes.Commands.UpdateRoute;
using EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRouteById;
using EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRoutes;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/routes")]
[Authorize]
public sealed class RoutesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoutesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetRoutes(
        [FromQuery] string? transportType,
        CancellationToken cancellationToken)
    {
        TransportType? type = null;

        if (!string.IsNullOrWhiteSpace(transportType)
            && PgEnumExtensions.TryFromPgName(transportType, out TransportType parsed))
        {
            type = parsed;
        }

        var routes = await _mediator.Send(
            new GetRoutesQuery(type),
            cancellationToken);

        return Ok(routes);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetRouteById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var route = await _mediator.Send(
            new GetRouteByIdQuery(id),
            cancellationToken);

        return Ok(route);
    }

    [HttpPost]
    [HasPermission("routes.write")]
    public async Task<IActionResult> CreateRoute(
        CreateRouteCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var commandWithUser = command with { CreatedBy = userId };

        var routeId = await _mediator.Send(
            commandWithUser,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetRouteById),
            new { id = routeId },
            new { id = routeId });
    }

    [HttpPut("{id:guid}")]
    [HasPermission("routes.write")]
    public async Task<IActionResult> UpdateRoute(
        Guid id,
        UpdateRouteCommand command,
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
    [HasPermission("routes.delete")]
    public async Task<IActionResult> DeleteRoute(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteRouteCommand(id),
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
