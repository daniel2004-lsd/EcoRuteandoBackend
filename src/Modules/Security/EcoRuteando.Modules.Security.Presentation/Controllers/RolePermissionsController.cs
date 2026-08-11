using EcoRuteando.Modules.Security.Application.RolePermissions.Commands.AssignPermissionToRole;
using EcoRuteando.Modules.Security.Application.RolePermissions.Commands.RemovePermissionFromRole;
using EcoRuteando.Modules.Security.Application.RolePermissions.Queries.GetRolePermissions;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/rolepermissions")]
[Authorize]
public sealed class RolePermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolePermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission("rolepermissions.assign")]

    public async Task<IActionResult> AssignPermission(
        AssignPermissionToRoleCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{roleId:int}/{permissionId:int}")]
    [HasPermission("rolepermissions.remove")]
    public async Task<IActionResult> RemovePermission(
        int roleId,
        int permissionId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RemovePermissionFromRoleCommand(roleId, permissionId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{roleId:int}")]
    [HasPermission("rolepermissions.read")]
    public async Task<IActionResult> GetPermissions(
        int roleId,
        CancellationToken cancellationToken)
    {
        var permissions = await _mediator.Send(
            new GetRolePermissionsQuery(roleId),
            cancellationToken);

        return Ok(permissions);
    }
}