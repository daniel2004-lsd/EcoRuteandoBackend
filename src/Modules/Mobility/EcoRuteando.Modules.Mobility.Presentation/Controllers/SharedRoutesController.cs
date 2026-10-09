using EcoRuteando.Modules.Mobility.Application.SharedRoutes.Commands.ShareRoute;
using EcoRuteando.Modules.Mobility.Application.SharedRoutes.Queries.GetSharedRoutes;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/shared-routes")]
[Authorize]
public sealed class SharedRoutesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SharedRoutesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// CU20: guarda el recorrido compartido con los datos autorizados.
    /// </summary>
    [HttpPost]
    [HasPermission("routes.write")]
    public async Task<IActionResult> ShareRoute(
        ShareRouteCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var commandWithUser = command with { UserId = userId };

        var id = await _mediator.Send(commandWithUser, cancellationToken);

        return CreatedAtAction(
            nameof(GetSharedRoutes),
            new { id },
            new { id });
    }

    /// <summary>
    /// Lista los recorridos compartidos por el usuario autenticado.
    /// </summary>
    [HttpGet]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetSharedRoutes(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var sharedRoutes = await _mediator.Send(
            new GetSharedRoutesQuery(userId),
            cancellationToken);

        return Ok(sharedRoutes);
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