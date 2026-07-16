using EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Security.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);

        return Ok(userId);
    }
}