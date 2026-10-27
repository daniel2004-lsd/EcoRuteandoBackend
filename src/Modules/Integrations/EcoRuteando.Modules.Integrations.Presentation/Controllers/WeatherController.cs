using EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestWeather;
using EcoRuteando.Modules.Integrations.Application.Environmental.Queries.GetWeather;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Integrations.Presentation.Controllers;

[ApiController]
[Route("api/integrations/weather")]
public class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Consulta el clima actual y pronóstico vigente para unas coordenadas geográficas.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetWeather(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double maxDistanceMeters = 50000,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetWeatherQuery(latitude, longitude, maxDistanceMeters), cancellationToken);
        if (result == null)
            return NotFound(new { message = "No hay datos meteorológicos vigentes para esta ubicación." });

        return Ok(result);
    }

    /// <summary>
    /// Registra una nueva lectura meteorológica desde un servicio o estación de clima.
    /// </summary>
    [HttpPost("ingest")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Ingest(
        [FromBody] IngestWeatherApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new IngestWeatherCommand(
            request.Latitude,
            request.Longitude,
            request.City,
            request.TemperatureC,
            request.HumidityPct,
            request.WindSpeedKmh,
            request.Condition,
            request.RainProbability,
            request.RawData,
            request.Source,
            request.ValidFrom,
            request.ValidUntil);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

public record IngestWeatherApiRequest(
    double Latitude,
    double Longitude,
    string? City,
    decimal? TemperatureC,
    short? HumidityPct,
    decimal? WindSpeedKmh,
    string? Condition,
    short? RainProbability,
    string? RawData,
    string? Source,
    DateTime ValidFrom,
    DateTime ValidUntil);
