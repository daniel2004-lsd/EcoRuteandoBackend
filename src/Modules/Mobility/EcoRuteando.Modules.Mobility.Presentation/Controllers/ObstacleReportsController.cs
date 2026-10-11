using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.ValidateObstacleReport;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.VoteObstacleReport;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Queries.GetActiveObstacleReports;
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
public sealed class ObstacleReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ObstacleReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// CU22: Consulta de reportes activos y con nivel de confianza suficiente para el mapa.
    /// Permite acceso a usuarios autenticados e invitados (modo lectura).
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(
        double? latitude,
        double? longitude,
        double? radiusKm,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetOptionalUserId();

        var reports = await _mediator.Send(
            new GetActiveObstacleReportsQuery(latitude, longitude, radiusKm, currentUserId),
            cancellationToken);

        return Ok(reports);
    }

    /// <summary>
    /// CU22: Confirmar o rechazar un reporte de obstáculo ("¿Sigue ocurriendo?").
    /// Requiere permiso 'reports.vote'.
    /// </summary>
    [HttpPost("{reportId:guid}/vote")]
    [Authorize]
    [HasPermission("reports.vote")]
    public async Task<IActionResult> Vote(
        Guid reportId,
        VoteReportApiRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var result = await _mediator.Send(
            new VoteObstacleReportCommand(
                reportId,
                request.Confirms,
                request.Latitude,
                request.Longitude,
                userId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// CU07: el ciudadano reporta un obstáculo o problema en las rutas.
    /// El reporte queda almacenado en estado 'pending' en espera de validación.
    /// </summary>
    [HttpPost]
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
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

    private Guid? GetOptionalUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}

/// <summary>
/// Cuerpo de petición para votar sobre un reporte comunitario (CU22).
/// </summary>
public sealed class VoteReportApiRequest
{
    public bool Confirms { get; init; }

    public double? Latitude { get; init; }

    public double? Longitude { get; init; }
}

/// <summary>
/// Cuerpo del endpoint de validación.
/// </summary>
public sealed class ValidateObstacleReportRequest
{
    public string? Status { get; init; }

    public string? ValidationNote { get; init; }
}