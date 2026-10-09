using EcoRuteando.Modules.Mobility.Application.Favorites.Commands.AddFavorite;
using EcoRuteando.Modules.Mobility.Application.Favorites.Commands.RemoveFavorite;
using EcoRuteando.Modules.Mobility.Application.Favorites.Queries.GetFavorites;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public sealed class FavoritesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FavoritesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetFavorites(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var favorites = await _mediator.Send(
            new GetFavoritesQuery(userId),
            cancellationToken);

        return Ok(favorites);
    }

    [HttpPost]
    [HasPermission("routes.write")]
    public async Task<IActionResult> AddFavorite(
        AddFavoriteCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var commandWithUser = command with { UserId = userId };

        await _mediator.Send(commandWithUser, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{routeId:guid}")]
    [HasPermission("routes.write")]
    public async Task<IActionResult> RemoveFavorite(
        Guid routeId,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        await _mediator.Send(
            new RemoveFavoriteCommand(routeId, userId),
            cancellationToken);

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
