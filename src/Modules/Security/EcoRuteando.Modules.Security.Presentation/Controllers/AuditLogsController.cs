using EcoRuteando.Modules.Security.Application.AuditLogs.Queries.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin,SuperAdmin")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        var logs = await _mediator.Send(
            new GetAuditLogsQuery(take),
            cancellationToken);

        return Ok(logs);
    }
}
