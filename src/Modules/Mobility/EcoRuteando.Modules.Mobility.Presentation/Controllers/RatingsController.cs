using EcoRuteando.Modules.Mobility.Application.Ratings.Commands.RateRoute;
using EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetMyRating;
using EcoRuteando.Modules.Mobility.Application.Ratings.Queries.GetRouteRatings;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/ratings")]
[Authorize]
public sealed class RatingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RatingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// CU09: guarda o actualiza la calificación del usuario para una ruta.
    /// </summary>
    [HttpPost]
    [HasPermission("routes.write")]
    public async Task<IActionResult> RateRoute(
        RateRouteCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var commandWithUser = command with { UserId = userId };

        await _mediator.Send(commandWithUser, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Lista las valoraciones de una ruta con su promedio.
    /// </summary>
    [HttpGet("route/{routeId:guid}")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetRouteRatings(
        Guid routeId,
        CancellationToken cancellationToken)
    {
        var ratings = await _mediator.Send(
            new GetRouteRatingsQuery(routeId),
            cancellationToken);

        return Ok(ratings);
    }

    /// <summary>
    /// Valoración del usuario autenticado para una ruta (null si no ha calificado).
    /// </summary>
    [HttpGet("route/{routeId:guid}/mine")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetMyRating(
        Guid routeId,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var rating = await _mediator.Send(
            new GetMyRatingQuery(routeId, userId),
            cancellationToken);

        return Ok(rating);
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