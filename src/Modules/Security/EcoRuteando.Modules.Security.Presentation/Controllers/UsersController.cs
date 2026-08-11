using EcoRuteando.Modules.Security.Application.Users.Commands.UpdateUser;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetUserById;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetUsers;
using EcoRuteando.Modules.Security.Presentation.Contracts.Users;
using EcoRuteando.Modules.Security.Application.Users.Commands.DeleteUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcoRuteando.Shared.Authorization;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] 
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("permissions.read")]
    public async Task<IActionResult> GetUsers(
        CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(
            new GetUsersQuery(),
            cancellationToken);

        return Ok(users);
    }

    
    [HttpGet("{id:guid}")]
    [HasPermission("permissions.read")]
    public async Task<IActionResult> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(
            new GetUserByIdQuery(id),
            cancellationToken);

        return Ok(user);
    }
    




    [HttpPut("{id:guid}")]
    [HasPermission("permissions.update ")]
    public async Task<IActionResult> UpdateUser(
     Guid id,
    UpdateUserRequest request,
    CancellationToken cancellationToken)
{
        var command = new UpdateUserCommand(

                id,
                request.FirstName,
                request.LastName,
                request.PhoneNumber


            );

        await _mediator.Send(command, cancellationToken);

        return NoContent();
}

    [HttpDelete("{id:guid}")]
    [HasPermission("permissions.delete")]
    public async Task<IActionResult> DeleteUser(
    Guid id,
    CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteUserCommand(id),
            cancellationToken);

        return NoContent();
    }





}