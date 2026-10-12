using EcoRuteando.Modules.Integrations.Application.Cache.Commands.InvalidateCache;
using EcoRuteando.Modules.Integrations.Application.Cache.Commands.PurgeExpiredCache;
using EcoRuteando.Modules.Integrations.Application.Cache.Commands.SetCachedResponse;
using EcoRuteando.Modules.Integrations.Application.Cache.Queries.GetRecentCacheEntries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Integrations.Presentation.Controllers;

[ApiController]
[Route("api/integrations/cache")]
[Authorize]
public class ApiCacheController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiCacheController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Consulta el listado de entradas recientes en la caché de APIs externas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecent(
        [FromQuery] string? provider,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRecentCacheEntriesQuery(provider, limit), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Almacena manualmente una respuesta en caché para un proveedor y endpoint.
    /// </summary>
    [HttpPost("set")]
    public async Task<IActionResult> SetCache(
        [FromBody] SetCacheApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetCachedResponseCommand(
            request.Provider,
            request.Endpoint,
            request.Parameters,
            request.Response,
            request.TtlSeconds);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Invalida una entrada específica de caché por ID o todas las entradas de un proveedor.
    /// </summary>
    [HttpPost("invalidate")]
    public async Task<IActionResult> Invalidate(
        [FromBody] InvalidateCacheApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new InvalidateCacheCommand(request.EntryId, request.Provider);
        var success = await _mediator.Send(command, cancellationToken);
        return Ok(new { invalidated = success });
    }

    /// <summary>
    /// Purga de la base de datos las entradas de caché vencidas o invalidadas.
    /// </summary>
    [HttpPost("purge-expired")]
    public async Task<IActionResult> PurgeExpired(CancellationToken cancellationToken)
    {
        var purgedCount = await _mediator.Send(new PurgeExpiredCacheCommand(), cancellationToken);
        return Ok(new { purgedCount });
    }
}

public record SetCacheApiRequest(
    string Provider,
    string Endpoint,
    string Parameters,
    string Response,
    int TtlSeconds = 3600);

public record InvalidateCacheApiRequest(
    long? EntryId = null,
    string? Provider = null);
