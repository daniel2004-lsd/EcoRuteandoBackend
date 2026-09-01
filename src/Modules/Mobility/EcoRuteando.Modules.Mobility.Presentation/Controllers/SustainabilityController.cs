using EcoRuteando.Modules.Mobility.Application.Sustainability.Queries.EstimateSustainability;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/sustainability")]
[Authorize]
public sealed class SustainabilityController : ControllerBase
{
    private readonly IMediator _mediator;

    public SustainabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Estima tiempo real (Google Maps) y CO₂/calorías ahorradas (factor de BD)
    /// para un trayecto entre origen y destino según el modo de transporte.
    /// Modos: bike, public_transport, mixed, walking.
    /// </summary>
    [HttpGet("estimate")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> Estimate(
        [FromQuery] double originLat,
        [FromQuery] double originLng,
        [FromQuery] double destinationLat,
        [FromQuery] double destinationLng,
        [FromQuery] string transportMode,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new EstimateSustainabilityQuery(
                originLat,
                originLng,
                destinationLat,
                destinationLng,
                transportMode),
            cancellationToken);

        return Ok(result);
    }
}
