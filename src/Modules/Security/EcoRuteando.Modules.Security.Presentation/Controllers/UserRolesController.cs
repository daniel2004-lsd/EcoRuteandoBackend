using EcoRuteando.Modules.Security.Application.UserRoles.Commands.AssignRoleToUser;
using EcoRuteando.Modules.Security.Application.UserRoles.Commands.RemoveRoleFromUser;
using EcoRuteando.Modules.Security.Application.UserRoles.Queries.GetUserRoles;
using EcoRuteando.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/userroles")]
[Authorize]
public sealed class UserRolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserRolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission("userroles.assign")]
    public async Task<IActionResult> AssignRole(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{userId:guid}/{roleId:int}")]
    [HasPermission("userroles.remove")]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        int roleId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RemoveRoleFromUserCommand(userId, roleId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{userId:guid}")]
    [HasPermission("userroles.read")]

    public async Task<IActionResult> GetUserRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var roles = await _mediator.Send(
            new GetUserRolesQuery(userId),
            cancellationToken);

        return Ok(roles);
    }
}