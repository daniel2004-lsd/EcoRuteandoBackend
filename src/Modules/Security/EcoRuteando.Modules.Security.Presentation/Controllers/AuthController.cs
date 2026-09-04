using EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginOAuth;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using EcoRuteando.Modules.Security.Application.Users.Commands.LogoutUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;
using EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;
using EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;
using EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;
using EcoRuteando.Modules.Security.Application.Users.Commands.VerifyEmail;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using EcoRuteando.Modules.Security.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

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

        [EnableRateLimiting("auth")]
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




        [EnableRateLimiting("auth")]
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

            return Ok(token);

        }


        [EnableRateLimiting("auth")]
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

        [EnableRateLimiting("sensitive")]
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

        [EnableRateLimiting("sensitive")]
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

        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification(
            ResendVerificationRequest? request,
            CancellationToken cancellationToken)
        {
            Guid? userId = null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is not null
                && Guid.TryParse(userIdClaim.Value, out var parsed))
            {
                userId = parsed;
            }

            await _mediator.Send(
                new SendVerificationEmailCommand(
                    userId,
                    request?.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString()),
                cancellationToken);

            return Ok(new
            {
                message = "Si el correo no fue verificado, se envió un código de verificación."
            });
        }

        [EnableRateLimiting("sensitive")]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(
            VerifyEmailRequest request,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new VerifyEmailCommand(request.Code),
                cancellationToken);

            return Ok(new
            {
                message = "Correo electrónico verificado correctamente."
            });
        }

        [EnableRateLimiting("auth")]
        [HttpPost("oauth/login")]
        public async Task<IActionResult> OAuthLogin(
            OAuthLoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new LoginWithOAuthCommand(
                request.Provider,
                request.AccessToken);

            var tokens = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                tokens.AccessToken,
                tokens.RefreshToken
            });
        }

        // ── Sessions ──────────────────────────────────────────────

        [Authorize]
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var sessions = await _mediator.Send(
                new GetActiveSessionsQuery(userId),
                cancellationToken);

            return Ok(sessions);
        }

        [Authorize]
        [HttpDelete("sessions/{sessionId:guid}")]
        public async Task<IActionResult> RevokeSession(
            Guid sessionId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _mediator.Send(
                new RevokeSessionCommand(userId, sessionId),
                cancellationToken);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("sessions")]
        public async Task<IActionResult> RevokeAllSessions(
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var count = await _mediator.Send(
                new RevokeAllSessionsCommand(userId),
                cancellationToken);

            return Ok(new { revoked = count });
        }

        // ── Two-Factor Authentication ─────────────────────────────

        [Authorize]
        [HttpPost("2fa/enable")]
        public async Task<IActionResult> EnableTwoFactorAuth(
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _mediator.Send(
                new EnableTwoFactorAuthCommand(userId),
                cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("2fa/verify")]
        public async Task<IActionResult> VerifyTwoFactorAuth(
            VerifyTwoFactorAuthRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _mediator.Send(
                new VerifyTwoFactorAuthCommand(userId, request.Code),
                cancellationToken);

            return Ok(new { message = "2FA activado correctamente." });
        }

        [Authorize]
        [HttpPost("2fa/disable")]
        public async Task<IActionResult> DisableTwoFactorAuth(
            DisableTwoFactorAuthRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _mediator.Send(
                new DisableTwoFactorAuthCommand(userId, request.Code),
                cancellationToken);

            return Ok(new { message = "2FA desactivado correctamente." });
        }
    }
}
