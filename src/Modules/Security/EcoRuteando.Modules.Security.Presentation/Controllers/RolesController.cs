using EcoRuteando.Shared.Authorization;
using EcoRuteando.Modules.Security.Application.Roles.Command.CreateRole;
using EcoRuteando.Modules.Security.Application.Roles.Command.DeleteRole;
using EcoRuteando.Modules.Security.Application.Roles.Command.UpdateRole;
using EcoRuteando.Modules.Security.Application.Roles.Commands.CreateRole;
using EcoRuteando.Modules.Security.Application.Roles.Commands.DeleteRole;
using EcoRuteando.Modules.Security.Application.Roles.Commands.UpdateRole;
using EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoleById;
using EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoles;
using EcoRuteando.Modules.Security.Presentation.Contracts.Role;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("roles.read")]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken)
    {
        var roles = await _mediator.Send(
            new GetRolesQuery(),
            cancellationToken);

        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    [HasPermission("roles.read")]
    public async Task<IActionResult> GetRoleById(
        int id,
        CancellationToken cancellationToken)
    {
        var role = await _mediator.Send(
            new GetRoleByIdQuery(id),
            cancellationToken);

        return Ok(role);
    }

    [HttpPost]
    [HasPermission("roles.create")]
    public async Task<IActionResult> CreateRole(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand(
            request.Name,
            request.Description);

        var roleId = await _mediator.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetRoleById),
            new { id = roleId },
            null);
    }

    [HttpPut("{id:int}")]
    [HasPermission("roles.update")]
    public async Task<IActionResult> UpdateRole(
        int id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoleCommand(
            id,
            request.Name,
            request.Description);

        await _mediator.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [HasPermission("roles.delete")]
    public async Task<IActionResult> DeleteRole(
        int id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteRoleCommand(id),
            cancellationToken);

        return NoContent();
    }
}