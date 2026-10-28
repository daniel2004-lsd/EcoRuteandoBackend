using System.Threading.RateLimiting;
using EcoRuteando.Api.Middlewares;
using EcoRuteando.Modules.Admin.Application;
using EcoRuteando.Modules.Admin.Infrastructure;
using EcoRuteando.Modules.Integrations.Application;
using EcoRuteando.Modules.Integrations.Infrastructure;
using EcoRuteando.Modules.Mobility.Application;
using EcoRuteando.Modules.Mobility.Infrastructure;
using EcoRuteando.Modules.Notifications.Application;
using EcoRuteando.Modules.Notifications.Infrastructure;
using EcoRuteando.Modules.Security.Application;
using EcoRuteando.Modules.Security.Infrastructure;
using EcoRuteando.Modules.Security.Infrastructure.Authorization;
using EcoRuteando.Modules.Security.Infrastructure.Bootstrap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace EcoRuteando.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Registrar Application
            builder.Services.AddSecurityApplication();
            builder.Services.AddMobilityApplication();
            builder.Services.AddNotificationsApplication();
            builder.Services.AddAdminApplication();
            builder.Services.AddIntegrationsApplication();

            // Registrar Infrastructure
            builder.Services.AddSecurityInfrastructure(builder.Configuration);
            builder.Services.AddMobilityInfrastructure(builder.Configuration);
            builder.Services.AddNotificationsInfrastructure(builder.Configuration);
            builder.Services.AddAdminInfrastructure(builder.Configuration);
            builder.Services.AddIntegrationsInfrastructure(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddHealthChecks();

            // Controllers + Swagger
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            // Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\":\"Demasiadas solicitudes. Por favor intente más tarde.\"}",
                        cancellationToken: cancellationToken);
                };

                // Helper local para resolver la clave de partición por IP:
                // No confía ciegamente en cabeceras arbitrarias sin validar.
                // context.Connection.RemoteIpAddress es procesado por Kestrel y ForwardedHeadersMiddleware.
                static string GetClientPartitionKey(HttpContext httpContext)
                {
                    var ip = httpContext.Connection.RemoteIpAddress;
                    return ip is not null ? ip.ToString() : "unknown_client_ip";
                }

                // Política para endpoints de auth: 20 requests por 5 min por IP.
                // El control de reintentos por cuenta (escalonado) vive en la lógica de
                // LoginUserCommandHandler; este límite por IP es una salvaguarda anti-fuerza-bruta por IP.
                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientPartitionKey(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20,
                            Window = TimeSpan.FromMinutes(5),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Política para forgot-password / send-verification / verify-email: 3 por hora por IP
                options.AddPolicy("sensitive", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientPartitionKey(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromHours(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Política general API: 100 por minuto por IP
                options.AddPolicy("api", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientPartitionKey(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });

            var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            var allowedOrigins = (configuredOrigins.Length > 0)
                ? configuredOrigins
                : (builder.Environment.IsDevelopment()
                    ? new[]
                    {
                        "http://localhost:3000",
                        "http://localhost:3001",
                        "http://localhost:3002",
                        "http://localhost:3007",
                        "http://localhost:5173"
                    }
                    : Array.Empty<string>());

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .WithHeaders("Authorization", "Content-Type", "Accept", "X-Client-Platform", "X-Requested-With", "X-CSRF-Token")
                            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                            .AllowCredentials();
                    }
                    else
                    {
                        policy
                            .WithHeaders("Authorization", "Content-Type", "Accept", "X-Client-Platform", "X-Requested-With", "X-CSRF-Token")
                            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS");
                    }
                });
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>

            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EcoRuteando API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Ingrese el JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });
            builder.Services.AddJwtAuthentication(builder.Configuration);





            var app = builder.Build();

            // Bootstrap del administrador inicial (Admin__*): crea el usuario,
            // le asigna 'Admin' como rol principal y verifica su correo.
            using (var scope = app.Services.CreateScope())
            {
                var adminBootstrap = scope.ServiceProvider
                    .GetRequiredService<IAdminBootstrapService>();

                await adminBootstrap.EnsureAdminAsync();
            }

            // 1. Manejador global de excepciones: captura cualquier fallo en middlewares posteriores
            app.UseMiddleware<ExceptionMiddleware>();

            // 2. Detrás de proxy inverso (Nginx, Traefik, ingress en nube): procesa X-Forwarded-For y X-Forwarded-Proto.
            // Para mitigar suplantación de IP (IP spoofing) y evasión de rate limiting:
            // - Solo se confía en proxies o redes CIDR explícitamente parametrizados (ForwardedHeaders:KnownProxies / KnownNetworks).
            // - Si se configuran proxies o redes personalizadas, se limpian los defaults y se aplican únicamente los autorizados.
            // - Si no se especifican proxies externos, se preservan las redes locales de loopback seguras por defecto (127.0.0.1, ::1).
            // - ForwardedLimit restringe la profundidad de cabeceras evaluadas para evitar cadenas manipuladas.
            var forwardedHeaders = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            var configuredLimit = builder.Configuration.GetValue<int?>("ForwardedHeaders:ForwardLimit")
                ?? builder.Configuration.GetValue<int?>("ForwardedHeaders:ForwardedLimit");
            forwardedHeaders.ForwardLimit = configuredLimit is > 0 ? configuredLimit.Value : 1;

            var configuredProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>()
                ?? Array.Empty<string>();
            var configuredNetworks = builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>()
                ?? Array.Empty<string>();

            bool hasCustomProxies = configuredProxies.Length > 0;
            bool hasCustomNetworks = configuredNetworks.Length > 0;

            if (hasCustomProxies || hasCustomNetworks)
            {
                forwardedHeaders.KnownProxies.Clear();
                forwardedHeaders.KnownNetworks.Clear();

                foreach (var proxy in configuredProxies)
                {
                    if (System.Net.IPAddress.TryParse(proxy.Trim(), out var ip))
                    {
                        forwardedHeaders.KnownProxies.Add(ip);
                    }
                }

                foreach (var network in configuredNetworks)
                {
                    var trimmed = network.Trim();
                    if (trimmed.Contains('/'))
                    {
                        var parts = trimmed.Split('/');
                        if (parts.Length == 2
                            && System.Net.IPAddress.TryParse(parts[0], out var prefix)
                            && int.TryParse(parts[1], out var prefixLength))
                        {
                            forwardedHeaders.KnownNetworks.Add(new IPNetwork(prefix, prefixLength));
                        }
                    }
                    else if (System.Net.IPAddress.TryParse(trimmed, out var singleIp))
                    {
                        int prefixLength = singleIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
                        forwardedHeaders.KnownNetworks.Add(new IPNetwork(singleIp, prefixLength));
                    }
                }
            }

            app.UseForwardedHeaders(forwardedHeaders);

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseMiddleware<SecurityHeadersMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("ReactPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapHealthChecks("/health").AllowAnonymous();

            app.MapControllers();

            app.Run();
        }
    }
}