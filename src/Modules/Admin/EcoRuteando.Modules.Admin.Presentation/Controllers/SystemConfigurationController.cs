using System.Security.Claims;
using EcoRuteando.Modules.Admin.Application.System.Commands.UpdateSystemConfiguration;
using EcoRuteando.Modules.Admin.Application.System.Queries.GetSystemConfiguration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Admin.Presentation.Controllers;

[ApiController]
[Route("api/admin/system-configuration")]
public class SystemConfigurationController : ControllerBase
{
    private readonly IMediator _mediator;

    public SystemConfigurationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene la configuración del sistema actual (colores, versiones de términos, etc.).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetConfiguration(CancellationToken cancellationToken)
    {
        var config = await _mediator.Send(new GetSystemConfigurationQuery(), cancellationToken);
        return Ok(config);
    }

    /// <summary>
    /// Consulta pública de la política de tratamiento de datos personales conforme a la Ley 1581 de 2012 (H-31).
    /// </summary>
    [HttpGet("privacy-policy")]
    [AllowAnonymous]
    public IActionResult GetPrivacyPolicy()
    {
        return Ok(new
        {
            title = "Política de Tratamiento de Datos Personales — EcoRuteando",
            legalFramework = "Ley Estatutaria 1581 de 2012 y Decreto 1377 de 2013 (Colombia)",
            responsibleParty = "EcoRuteando (Sena ADSO Neiva)",
            contactEmail = "privacidad@ecoruteando.org",
            rights = new[] { "Conocer", "Actualizar", "Rectificar", "Suprimir", "Revocar autorización" },
            securityMeasures = "BCrypt factor 12, AES-256-GCM para 2FA, JWT con rotación y HTTPS.",
            version = "1.0",
            lastUpdated = "2026-10-31"
        });
    }

    /// <summary>
    /// Actualiza la configuración global del sistema (requiere privilegios administrativos).
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateConfiguration(
        [FromBody] UpdateSystemConfigurationApiRequest request,
        CancellationToken cancellationToken)
    {
        Guid? userId = null;
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (Guid.TryParse(sub, out var parsed))
        {
            userId = parsed;
        }

        var command = new UpdateSystemConfigurationCommand(
            request.AppName,
            request.AdminEmail,
            request.SupportPhone,
            request.TermsVersion,
            request.TermsUrl,
            request.PrivacyVersion,
            request.PrivacyUrl,
            request.EnabledApis,
            request.MapCacheTtlSec,
            request.WeatherCacheTtlSec,
            request.PrimaryColor,
            request.SecondaryColor,
            request.LogoUrl,
            userId);

        var updated = await _mediator.Send(command, cancellationToken);
        return Ok(updated);
    }
}

public record UpdateSystemConfigurationApiRequest(
    string AppName,
    string? AdminEmail,
    string? SupportPhone,
    string? TermsVersion,
    string? TermsUrl,
    string? PrivacyVersion,
    string? PrivacyUrl,
    string? EnabledApis,
    int MapCacheTtlSec,
    int WeatherCacheTtlSec,
    string PrimaryColor,
    string SecondaryColor,
    string? LogoUrl);
