using EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestAirQuality;
using EcoRuteando.Modules.Integrations.Application.Environmental.Queries.GetAirQuality;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Integrations.Presentation.Controllers;

[ApiController]
[Route("api/integrations/air-quality")]
public class AirQualityController : ControllerBase
{
    private readonly IMediator _mediator;

    public AirQualityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Consulta el índice de calidad del aire (AQI) y concentración de contaminantes más reciente cerca a una coordenada.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAirQuality(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double maxDistanceMeters = 50000,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAirQualityQuery(latitude, longitude, maxDistanceMeters), cancellationToken);
        if (result == null)
            return NotFound(new { message = "No hay mediciones recientes de calidad del aire en esta zona." });

        return Ok(result);
    }

    /// <summary>
    /// Registra una nueva lectura de calidad del aire desde una estación o servicio externo.
    /// </summary>
    [HttpPost("ingest")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Ingest(
        [FromBody] IngestAirQualityApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new IngestAirQualityCommand(
            request.Latitude,
            request.Longitude,
            request.City,
            request.Aqi,
            request.Pm25,
            request.Pm10,
            request.Co2Ppm,
            request.Source,
            request.RawData,
            request.MeasuredAt);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

public record IngestAirQualityApiRequest(
    double Latitude,
    double Longitude,
    string? City,
    int? Aqi,
    decimal? Pm25,
    decimal? Pm10,
    decimal? Co2Ppm,
    string? Source,
    string? RawData,
    DateTime? MeasuredAt);
