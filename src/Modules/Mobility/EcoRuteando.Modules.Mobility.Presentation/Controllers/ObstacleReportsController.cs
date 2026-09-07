using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.ValidateObstacleReport;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetMyObstacleReports;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetObstacleReportsForValidation;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/obstacle-reports")]
[Authorize]
public sealed class ObstacleReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ObstacleReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// CU07: el ciudadano reporta un obstáculo o problema en las rutas.
    /// El reporte queda almacenado en estado 'pending' en espera de validación.
    /// </summary>
    [HttpPost]
    [HasPermission("reports.create")]
    public async Task<IActionResult> Create(
        CreateObstacleReportCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var reportId = await _mediator.Send(
            command with { UserId = userId },
            cancellationToken);

        return Ok(new { reportId });
    }

    /// <summary>
    /// Reportes del usuario autenticado con su estado de validación.
    /// </summary>
    [HttpGet("mine")]
    [HasPermission("reports.read")]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var reports = await _mediator.Send(
            new GetMyObstacleReportsQuery(userId),
            cancellationToken);

        return Ok(reports);
    }

    /// <summary>
    /// Listado administrativo de reportes (filtrable por estado).
    /// </summary>
    [HttpGet]
    [HasPermission("reports.read")]
    public async Task<IActionResult> GetAll(
        string? status,
        CancellationToken cancellationToken)
    {
        ReportStatus? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(status)
            && PgEnumExtensions.TryFromPgName(status, out ReportStatus parsed))
        {
            statusFilter = parsed;
        }

        var reports = await _mediator.Send(
            new GetObstacleReportsForValidationQuery(statusFilter),
            cancellationToken);

        return Ok(reports);
    }

    /// <summary>
    /// CU07 (actor Administrador): valida o rechaza un reporte.
    /// </summary>
    [HttpPatch("{reportId:guid}/status")]
    [HasPermission("reports.validate")]
    public async Task<IActionResult> Validate(
        Guid reportId,
        ValidateObstacleReportRequest request,
        CancellationToken cancellationToken)
    {
        var validatorId = GetRequiredUserId();

        await _mediator.Send(
            new ValidateObstacleReportCommand(
                reportId,
                request.Status ?? string.Empty,
                request.ValidationNote,
                validatorId),
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

/// <summary>
/// Cuerpo del endpoint de validación.
/// </summary>
public sealed class ValidateObstacleReportRequest
{
    public string? Status { get; init; }

    public string? ValidationNote { get; init; }
}