using EcoRuteando.Modules.Admin.Application.Analytics.Commands.UpsertDailyStatistic;
using EcoRuteando.Modules.Admin.Application.Analytics.Queries.GetDailyStatistics;
using EcoRuteando.Modules.Admin.Application.Analytics.Queries.GetStatisticsSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Admin.Presentation.Controllers;

[ApiController]
[Route("api/admin/statistics")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene las estadísticas agregadas por día en un rango de fechas.
    /// </summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDailyStatisticsQuery(from, to), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el resumen global más reciente de la plataforma.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatisticsSummaryQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra o actualiza el consolidado de estadísticas para una fecha.
    /// </summary>
    [HttpPost("daily")]
    public async Task<IActionResult> UpsertDaily(
        [FromBody] UpsertDailyStatisticApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpsertDailyStatisticCommand(
            request.StatisticDate,
            request.TotalUsers,
            request.NewUsers,
            request.ActiveUsers,
            request.TotalRoutes,
            request.CompletedRoutes,
            request.TotalCommunities,
            request.TotalReports,
            request.TotalCo2Saved,
            request.TotalDistanceKm);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

public record UpsertDailyStatisticApiRequest(
    DateOnly StatisticDate,
    int TotalUsers,
    int NewUsers,
    int ActiveUsers,
    int TotalRoutes,
    int CompletedRoutes,
    int TotalCommunities,
    int TotalReports,
    decimal TotalCo2Saved,
    decimal TotalDistanceKm);
