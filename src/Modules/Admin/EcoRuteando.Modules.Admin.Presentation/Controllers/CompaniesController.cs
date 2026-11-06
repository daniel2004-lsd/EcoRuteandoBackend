using EcoRuteando.Modules.Admin.Application.Sustainability.Commands.CompensateCompanyCarbon;
using EcoRuteando.Modules.Admin.Application.Sustainability.Dtos;
using EcoRuteando.Modules.Admin.Presentation.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Admin.Presentation.Controllers;

[ApiController]
[Route("api/v1/admin/companies")]
[Route("api/admin/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra una acción de compensación voluntaria de huella de carbono para una empresa de transporte.
    /// Calcula equivalencia en árboles nativos y emite código de verificación oficial para la autoridad ambiental.
    /// </summary>
    /// <param name="id">Identificador único de la empresa (Tenant ID)</param>
    /// <param name="request">Datos del volumen en kg de CO2 a compensar voluntariamente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Certificado y detalle de la compensación registrada</returns>
    [HttpPost("{id:guid}/compensate")]
    [Authorize(Roles = "Admin,SuperAdmin,admin,gestor_ambiental")]
    [ProducesResponseType(typeof(CompensateCompanyCarbonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compensate(
        [FromRoute] Guid id,
        [FromBody] CompensateCompanyCarbonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompensateCompanyCarbonCommand(
            CompanyId: id,
            Co2CompensatedKg: request.Co2CompensatedKg,
            PartnerProgram: request.PartnerProgram,
            Notes: request.Notes);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
