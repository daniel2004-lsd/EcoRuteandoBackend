using System.Security.Claims;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EcoRuteando.Modules.Security.Infrastructure;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection("Jwt")
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("La configuración Jwt no existe.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        if (principal == null)
                        {
                            context.Fail("Principal no disponible.");
                            return;
                        }

                        var tokenType = principal.FindFirst("token_type")?.Value;
                        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                        // Los tokens de dispositivo confiable jamás son válidos como tokens de acceso Bearer
                        if (tokenType == "trusted_device")
                        {
                            context.Fail("Token de dispositivo confiable no puede utilizarse como token de acceso.");
                            return;
                        }

                        // Los tokens temporales de 2FA únicamente se admiten para completar la verificación de 2FA
                        var isTwoFactorTemp = tokenType == "two_factor_temp" || role == "two_factor_pending";
                        var requestPath = (context.HttpContext.Request.Path.Value ?? string.Empty).TrimEnd('/');
                        var is2FaEndpoint = requestPath.EndsWith("/api/auth/2fa/verify", StringComparison.OrdinalIgnoreCase);

                        if (isTwoFactorTemp && !is2FaEndpoint)
                        {
                            context.Fail("Token temporal de 2FA no autorizado para acceder a este recurso.");
                            return;
                        }

                        // SEC-03: Validación activa de sesión para tokens de acceso
                        if (tokenType == "access")
                        {
                            var sessionIdClaim = principal.FindFirst("session_id")?.Value;
                            if (string.IsNullOrWhiteSpace(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
                            {
                                context.Fail("Token de acceso sin identificador de sesión válido.");
                                return;
                            }

                            var sessionRepository = context.HttpContext.RequestServices.GetService<ISessionRepository>();
                            if (sessionRepository == null)
                            {
                                context.Fail("Servicio de validación de sesiones no disponible.");
                                return;
                            }

                            var session = await sessionRepository.GetByIdAsync(sessionId, context.HttpContext.RequestAborted);
                            if (session == null || !session.IsActive || session.ExpiresAt < DateTime.UtcNow)
                            {
                                context.Fail("La sesión ha sido revocada o ha expirado.");
                                return;
                            }
                        }
                    }
                };
            });

        return services;
    }
}
