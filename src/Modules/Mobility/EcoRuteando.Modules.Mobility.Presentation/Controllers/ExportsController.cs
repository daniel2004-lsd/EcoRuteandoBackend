using EcoRuteando.Modules.Mobility.Application.Exports;
using EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetStatsExport;
using EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetTripsExport;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

[ApiController]
[Route("api/exports")]
[Authorize]
public sealed class ExportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Exporta el historial de trayectos del usuario autenticado (CU19).
    /// Formatos admitidos: csv, json, xlsx.
    /// Opcional: from/to filtran por fecha de inicio del trayecto (RF29.2).
    /// </summary>
    [HttpGet("trips")]
    [HasPermission("routes.read")]
    public async Task<IActionResult> ExportTrips(
        [FromQuery] string format,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (!TryParseFormat(format, out var exportFormat))
        {
            return BadRequest(new { message = "Formato no válido. Usa csv, json o xlsx." });
        }

        var userId = GetRequiredUserId();

        var file = await _mediator.Send(
            new GetTripsExportQuery(userId, exportFormat, from, to),
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    /// <summary>
    /// Exporta las estadísticas del portal de administración (CU08/CU19).
    /// Formatos admitidos: csv, json, xlsx.
    /// </summary>
    [HttpGet("stats")]
    [HasPermission("stats.read")]
    public async Task<IActionResult> ExportStats(
        [FromQuery] string format,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (!TryParseFormat(format, out var exportFormat))
        {
            return BadRequest(new { message = "Formato no válido. Usa csv, json o xlsx." });
        }

        var file = await _mediator.Send(
            new GetStatsExportQuery(from, to, exportFormat),
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    private static bool TryParseFormat(
        string? format,
        out ExportFormat exportFormat)
    {
        exportFormat = default;

        return Enum.TryParse(format, ignoreCase: true, out exportFormat)
            && Enum.IsDefined(typeof(ExportFormat), exportFormat);
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