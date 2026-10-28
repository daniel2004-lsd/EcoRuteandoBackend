using EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.ChangeMyPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginOAuth;
using EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers;
using EcoRuteando.Modules.Security.Application.Users.Commands.LogoutUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.RefreshToken;
using EcoRuteando.Modules.Security.Application.Users.Commands.RegisterUser;
using EcoRuteando.Modules.Security.Application.Users.Commands.ResetPassword;
using EcoRuteando.Modules.Security.Application.Users.Commands.SendVerificationEmail;
using EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;
using EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;
using EcoRuteando.Modules.Security.Application.Users.Commands.TwoFactorAuth;
using EcoRuteando.Modules.Security.Application.Users.Commands.VerifyEmail;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetCurrentUser;
using EcoRuteando.Modules.Security.Application.Users.Queries.GetEmailVerificationStatus;
using EcoRuteando.Modules.Security.Presentation.Contracts.Auth;
using EcoRuteando.Modules.Security.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace EcoRuteando.Modules.Security.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";

        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            IMediator mediator,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _mediator = mediator;
            _configuration = configuration;
            _environment = environment;
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

            var result = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(new
            {
                userId = result.UserId,
                message = result.Message
            });
        }

        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new LoginUserCommand(
                request.Email,
                request.Password,
                request.TrustedDeviceToken);

            var token = await _mediator.Send(
                command,
                cancellationToken);

            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                SetRefreshTokenCookie(token.RefreshToken);
            }

            if (IsWebClient())
            {
                return Ok(new
                {
                    accessToken = token.AccessToken,
                    requiresTwoFactor = token.RequiresTwoFactor,
                    twoFactorToken = token.TwoFactorToken,
                    requiresEmailVerification = token.RequiresEmailVerification
                });
            }

            return Ok(token);
        }

        [EnableRateLimiting("auth")]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            RefreshTokenRequest? request,
            CancellationToken cancellationToken)
        {
            var hasCookie = Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieToken)
                && !string.IsNullOrWhiteSpace(cookieToken);

            var tokenFromRequest = request?.RefreshToken;
            var isCookieAuth = string.IsNullOrWhiteSpace(tokenFromRequest) && hasCookie;
            var refreshToken = isCookieAuth ? cookieToken : tokenFromRequest;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new { message = "Refresh token no proporcionado o cookie ausente." });
            }

            if (isCookieAuth)
            {
                var origin = GetRequestOrigin();
                if (string.IsNullOrEmpty(origin) || !IsAllowedOrigin(origin))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { error = "Origen no autorizado o solicitud CSRF rechazada." });
                }

                if (!HasAntiCsrfHeader())
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { error = "Cabecera anti-CSRF ausente." });
                }
            }

            var command = new RefreshTokenCommand(refreshToken);

            var tokens = await _mediator.Send(
                command,
                cancellationToken);

            SetRefreshTokenCookie(tokens.RefreshToken);

            if (IsWebClient() || isCookieAuth)
            {
                return Ok(new
                {
                    accessToken = tokens.AccessToken
                });
            }

            return Ok(tokens);
        }

        [EnableRateLimiting("auth")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            LogoutRequest? request,
            CancellationToken cancellationToken)
        {
            var hasCookie = Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieToken)
                && !string.IsNullOrWhiteSpace(cookieToken);

            var refreshToken = !string.IsNullOrWhiteSpace(request?.RefreshToken)
                ? request.RefreshToken
                : (hasCookie ? cookieToken : null);

            DeleteRefreshTokenCookie();

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                try
                {
                    await _mediator.Send(
                        new LogoutUserCommand(refreshToken),
                        cancellationToken);
                }
                catch
                {
                    // Ignore revocation error if token was already expired or invalid
                }
            }

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
                return Unauthorized();
            }

            var query = new GetCurrentUserQuery(
                Guid.Parse(userIdClaim.Value));

            var user = await _mediator.Send(
                query,
                cancellationToken);

            return Ok(user);
        }

        // Rate limiting por IP particionado (sensitive: 3/hora por IP) combinado
        // con el rate limiter escalonado por email en el handler (IForgotPasswordRateLimiter).
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
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    request.Email),
                cancellationToken);

            return Ok(new
            {
                message = "La contraseña fue actualizada correctamente."
            });
        }

        /// <summary>
        /// El usuario autenticado cambia su contraseña confirmando la actual (CU12).
        /// </summary>
        [Authorize]
        [EnableRateLimiting("sensitive")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            ChangeMyPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                return Unauthorized();
            }

            await _mediator.Send(
                new ChangeMyPasswordCommand(
                    Guid.Parse(userIdClaim.Value),
                    request.CurrentPassword,
                    request.NewPassword),
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

            var alreadyVerified = await _mediator.Send(
                new SendVerificationEmailCommand(
                    userId,
                    request?.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString()),
                cancellationToken);

            if (alreadyVerified)
            {
                return Ok(new
                {
                    message = "El correo electrónico ya está verificado.",
                    alreadyVerified = true
                });
            }

            return Ok(new
            {
                message = "Si el correo no fue verificado, se envió un código de verificación.",
                alreadyVerified = false
            });
        }

        /// <summary>
        /// Indica si un correo electrónico ya está verificado, para que la UI
        /// pueda mostrar "cuenta ya activada" en lugar del formulario de código.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpGet("verification-status")]
        public async Task<IActionResult> GetVerificationStatus(
            [FromQuery] string? email,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetEmailVerificationStatusQuery(email),
                cancellationToken);

            return Ok(result);
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

            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                SetRefreshTokenCookie(tokens.RefreshToken);
            }

            if (IsWebClient())
            {
                return Ok(new
                {
                    accessToken = tokens.AccessToken
                });
            }

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
        [EnableRateLimiting("auth")]
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
        [EnableRateLimiting("auth")]
        [HttpPost("2fa/verify")]
        public async Task<IActionResult> VerifyTwoFactorAuth(
            VerifyTwoFactorAuthRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _mediator.Send(
                new VerifyTwoFactorAuthCommand(userId, request.Code, request.RememberDevice),
                cancellationToken);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            if (IsWebClient())
            {
                return Ok(new
                {
                    message = result.Message,
                    accessToken = result.AccessToken,
                    trustedDeviceToken = result.TrustedDeviceToken
                });
            }

            return Ok(result);
        }

        [Authorize]
        [EnableRateLimiting("auth")]
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

        // ── Trusted Devices ───────────────────────────────────────

        [Authorize]
        [HttpGet("trusted-devices")]
        public async Task<IActionResult> GetTrustedDevices(
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var devices = await _mediator.Send(
                new GetTrustedDevicesQuery(userId),
                cancellationToken);

            return Ok(devices);
        }

        [Authorize]
        [HttpDelete("trusted-devices/{deviceId:guid}")]
        public async Task<IActionResult> RevokeTrustedDevice(
            Guid deviceId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _mediator.Send(
                new RevokeTrustedDeviceCommand(userId, deviceId),
                cancellationToken);

            return NoContent();
        }

        // ── Private Cookie & CSRF Helpers ─────────────────────────

        private CookieOptions CreateRefreshTokenCookieOptions(DateTimeOffset? expires = null)
        {
            var isDevelopment = _environment.IsDevelopment();
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment || Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth",
                Expires = expires ?? DateTimeOffset.UtcNow.AddDays(7)
            };
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, CreateRefreshTokenCookieOptions());
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete(RefreshTokenCookieName, CreateRefreshTokenCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
        }

        private bool IsWebClient()
        {
            if (Request.Headers.TryGetValue("X-Client-Platform", out var platform) &&
                string.Equals(platform, "web", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Request.Headers.ContainsKey("Origin") || Request.Headers.ContainsKey("Referer"))
            {
                return true;
            }

            return false;
        }

        private string? GetRequestOrigin()
        {
            if (Request.Headers.TryGetValue("Origin", out var originHeader) && !string.IsNullOrWhiteSpace(originHeader))
            {
                return originHeader.ToString();
            }

            if (Request.Headers.TryGetValue("Referer", out var refererHeader) && !string.IsNullOrWhiteSpace(refererHeader))
            {
                if (Uri.TryCreate(refererHeader, UriKind.Absolute, out var refererUri))
                {
                    return $"{refererUri.Scheme}://{refererUri.Authority}";
                }
            }

            return null;
        }

        private bool IsAllowedOrigin(string origin)
        {
            var configuredOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            var allowedOrigins = configuredOrigins.Length > 0
                ? configuredOrigins
                : (_environment.IsDevelopment()
                    ? new[]
                    {
                        "http://localhost:3000",
                        "http://localhost:3001",
                        "http://localhost:3002",
                        "http://localhost:3007",
                        "http://localhost:5173"
                    }
                    : Array.Empty<string>());

            return allowedOrigins.Any(o => string.Equals(o.TrimEnd('/'), origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
        }

        private bool HasAntiCsrfHeader()
        {
            if (Request.Headers.TryGetValue("X-Client-Platform", out var plat)
                && string.Equals(plat, "web", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Request.Headers.ContainsKey("X-Requested-With") || Request.Headers.ContainsKey("X-CSRF-Token"))
            {
                return true;
            }

            return false;
        }
    }
}
