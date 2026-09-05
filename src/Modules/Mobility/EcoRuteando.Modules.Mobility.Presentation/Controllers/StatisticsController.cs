using EcoRuteando.Modules.Mobility.Application.Statistics.Queries.GetStats;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize]
public sealed class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Estadísticas agregadas del sistema (portal de administración, CU08):
    /// rutas consultadas, usuarios activos, CO₂ ahorrado y desgloses
    /// mensuales / por modo de transporte. Filtro por rango de fechas opcional.
    /// </summary>
    [HttpGet]
    [HasPermission("stats.read")]
    public async Task<IActionResult> GetStats(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStatsQuery(from, to),
            cancellationToken);

        return Ok(result);
    }
}