using EcoRuteando.Modules.Admin.Application.Factors.Commands.CreateTransportFactor;
using EcoRuteando.Modules.Admin.Application.Factors.Commands.UpdateTransportFactor;
using EcoRuteando.Modules.Admin.Application.Factors.Queries.GetTransportFactors;
using EcoRuteando.Modules.Admin.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Admin.Presentation.Controllers;

[ApiController]
[Route("api/admin/transport-factors")]
public class TransportFactorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransportFactorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista todos los factores de transporte y emisión registrados.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        var factors = await _mediator.Send(new GetTransportFactorsQuery(onlyActive), cancellationToken);
        return Ok(factors);
    }

    /// <summary>
    /// Obtiene los factores de transporte y emisión vigentes (accesible para cálculo de rutas/CO2).
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var factors = await _mediator.Send(new GetTransportFactorsQuery(OnlyActive: true), cancellationToken);
        return Ok(factors);
    }

    /// <summary>
    /// Registra un nuevo factor de emisión para un medio de transporte.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransportFactorApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransportFactorCommand(
            request.TransportType,
            request.Co2FactorKgKm,
            request.CalorieFactorKm,
            request.ValidFrom,
            request.ValidUntil,
            request.Source);

        var created = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, created);
    }

    /// <summary>
    /// Actualiza un factor de emisión existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTransportFactorApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTransportFactorCommand(
            id,
            request.Co2FactorKgKm,
            request.CalorieFactorKm,
            request.ValidFrom,
            request.ValidUntil,
            request.Source);

        var updated = await _mediator.Send(command, cancellationToken);
        return Ok(updated);
    }
}

public record CreateTransportFactorApiRequest(
    TransportType TransportType,
    decimal Co2FactorKgKm,
    decimal? CalorieFactorKm,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? Source);

public record UpdateTransportFactorApiRequest(
    decimal Co2FactorKgKm,
    decimal? CalorieFactorKm,
    DateTime ValidFrom,
    DateTime? ValidUntil,
    string? Source);
