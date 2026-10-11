using System.Security.Claims;
using EcoRuteando.Modules.Admin.Application.Exports.Commands.RequestExport;
using EcoRuteando.Modules.Admin.Application.Exports.Commands.UpdateExportStatus;
using EcoRuteando.Modules.Admin.Application.Exports.Queries.GetExportById;
using EcoRuteando.Modules.Admin.Application.Exports.Queries.GetRecentExports;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Admin.Presentation.Controllers;

[ApiController]
[Route("api/admin/exports")]
[Authorize]
public class ExportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Solicita la generación de un nuevo reporte exportable (PDF, XLSX, CSV, JSON).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RequestExport(
        [FromBody] RequestExportApiRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new RequestExportCommand(
            userId,
            request.ExportType,
            request.Format,
            request.Filters);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista el historial de exportaciones recientes.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecent(
        [FromQuery] bool mineOnly = false,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = mineOnly ? GetCurrentUserId() : null;
        var result = await _mediator.Send(new GetRecentExportsQuery(userId, limit), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta el estado y enlace de descarga de una exportación.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExportByIdQuery(id), cancellationToken);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Actualiza el estado de una exportación generada por el procesador de reportes.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateExportStatusApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExportStatusCommand(
            id,
            request.Status,
            request.FileUrl,
            request.SizeBytes,
            request.RecordsCount,
            request.ErrorMessage);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var parsed) ? parsed : null;
    }
}

public record RequestExportApiRequest(
    string ExportType,
    ExportFormat Format,
    string? Filters = null);

public record UpdateExportStatusApiRequest(
    ExportStatus Status,
    string? FileUrl = null,
    long? SizeBytes = null,
    int? RecordsCount = null,
    string? ErrorMessage = null);
