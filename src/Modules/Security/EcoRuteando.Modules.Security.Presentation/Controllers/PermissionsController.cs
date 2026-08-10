using EcoRuteando.Modules.Security.Application.Permissions.Commands.CreatePermission;
using EcoRuteando.Modules.Security.Application.Permissions.Commands.DeletePermission;
using EcoRuteando.Modules.Security.Application.Permissions.Commands.UpdatePermission;
using EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissionById;
using EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissions;
using EcoRuteando.Modules.Security.Presentation.Contracts.Permission;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public sealed class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("permissions.read")]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken)
    {
        var permissions = await _mediator.Send(
            new GetPermissionsQuery(),
            cancellationToken);

        return Ok(permissions);
    }

    [HttpGet("{id:int}")]
    [HasPermission("permissions.read")]
    public async Task<IActionResult> GetPermissionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var permission = await _mediator.Send(
            new GetPermissionByIdQuery(id),
            cancellationToken);

        return Ok(permission);
    }

    [HttpPost]
    [HasPermission("permissions.create")]
    public async Task<IActionResult> CreatePermission(
        CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePermissionCommand(
            request.Name,
            request.Description);

        var permissionId = await _mediator.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetPermissionById),
            new { id = permissionId },
            null);
    }

    [HttpPut("{id:int}")]
    [HasPermission("permissions.update")]
    public async Task<IActionResult> UpdatePermission(
        Guid id,
        UpdatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePermissionCommand(
            id,
            request.Name,
            request.Description);

        await _mediator.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [HasPermission("permissions.delete")]
    public async Task<IActionResult> DeletePermission(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeletePermissionCommand(id),
            cancellationToken);

        return NoContent();
    }
}