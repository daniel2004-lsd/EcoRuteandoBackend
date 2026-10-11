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
    /// Actualiza la configuración global del sistema (requiere privilegios administrativos).
    /// </summary>
    [HttpPut]
    [Authorize]
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
