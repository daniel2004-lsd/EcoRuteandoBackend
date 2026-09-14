using EcoRuteando.Modules.Security.Application.Users.Commands.UpdateUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyProfile;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetUserById;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetUsers;
using EcoRuteando.Modules.Security.Presentation.Contracts.Users;
using EcoRuteando.Modules.Security.Application.Users.Commands.DeleteUser;
using EcoRuteando.Modules.Security.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [HasPermission("users.read")]
    public async Task<IActionResult> GetUsers(
        CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(
            new GetUsersQuery(),
            cancellationToken);

        return Ok(users);
    }

    
    [HttpGet("{id:guid}")]
    [HasPermission("users.read")]
    public async Task<IActionResult> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(
            new GetUserByIdQuery(id),
            cancellationToken);

        return Ok(user);
    }
    




    /// <summary>
    /// El usuario autenticado actualiza su propio perfil confirmando su
    /// identidad con la contraseña actual (CU12/RF5).
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new UpdateMyProfileCommand(
            userId.Value,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.CurrentPassword);

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("users.update")]
    public async Task<IActionResult> UpdateUser(
     Guid id,
    UpdateUserRequest request,
    CancellationToken cancellationToken)
{
        var command = new UpdateUserCommand(

                id,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.PrimaryColor


            );

        await _mediator.Send(command, cancellationToken);

        return NoContent();
}

    [HttpDelete("{id:guid}")]
    [HasPermission("users.delete")]
    public async Task<IActionResult> DeleteUser(
    Guid id,
    CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteUserCommand(id),
            cancellationToken);

        return NoContent();
    }

    private Guid? GetAuthenticatedUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim is not null && Guid.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}
