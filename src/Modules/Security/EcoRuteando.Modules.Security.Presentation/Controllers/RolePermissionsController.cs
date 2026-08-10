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

    [HttpDelete("{roleId:Guid}/{permissionId:Guid}")]
    [HasPermission("rolepermissions.remove")]
    public async Task<IActionResult> RemovePermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RemovePermissionFromRoleCommand(roleId, permissionId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{roleId:Guid}")]
    [HasPermission("rolepermissions.read")]
    public async Task<IActionResult> GetPermissions(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var permissions = await _mediator.Send(
            new GetRolePermissionsQuery(roleId),
            cancellationToken);

        return Ok(permissions);
    }
}