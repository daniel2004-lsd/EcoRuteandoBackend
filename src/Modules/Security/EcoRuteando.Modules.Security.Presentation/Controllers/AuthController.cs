using EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using EcoRuteando.Modules.Security.Application.Users.Commands.LogoutUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;
using EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using EcoRuteando.Modules.Security.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password);

            var userId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(userId);
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new LoginUserCommand(
                request.Email,
                request.Password);

            var token = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                Token = token
            });

        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
        {
            var command = new RefreshTokenCommand(
                request.RefreshToken);

            var tokens = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(tokens);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new LogoutUserCommand(request.RefreshToken),
                cancellationToken);

            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return   Unauthorized();
            }

            var query = new GetCurrentUserQuery(
                Guid.Parse(userIdClaim.Value));

            var user = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(user);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new ForgotPasswordCommand(
                    request.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString()),
                cancellationToken);

            return Ok(new
            {
                message = "Si el correo existe, se enviará un enlace para autentificar la contraseña."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new ResetPasswordCommand(
                    request.Token,
                    request.NewPassword,
                    HttpContext.Connection.RemoteIpAddress?.ToString()),
                cancellationToken);

            return Ok(new
            {
                message = "La contraseña fue actualizada correctamente."
            });
        }
    }
}
