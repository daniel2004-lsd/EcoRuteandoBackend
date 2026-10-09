using EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeather;
using EcoRuteando.Modules.Mobility.Application.Weather.Queries.GetWeatherCurrent;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/weather")]
[Authorize]
public sealed class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Condiciones climáticas y alertas activas (Google Weather API) para un trayecto.
    /// Nunca bloquea: si el servicio climático falla, devuelve una respuesta vacía.
    /// </summary>
    [HttpGet("route")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetRouteWeather(
        [FromQuery] double originLat,
        [FromQuery] double originLng,
        [FromQuery] double destinationLat,
        [FromQuery] double destinationLng,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetWeatherQuery(originLat, originLng, destinationLat, destinationLng),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Condiciones climáticas actuales de un punto específico (ej. Neiva) con sus alertas.
    /// </summary>
    [HttpGet("current")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> GetCurrent(
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetWeatherCurrentQuery(lat, lng),
            cancellationToken);

        return Ok(result);
    }
}