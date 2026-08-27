using System.Threading.RateLimiting;
using EcoRuteando.Api.Middlewares;
using EcoRuteando.Modules.Mobility.Application;
using EcoRuteando.Modules.Mobility.Infrastructure;
using EcoRuteando.Modules.Security.Application;
using EcoRuteando.Modules.Security.Infrastructure;
using EcoRuteando.Modules.Security.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

namespace EcoRuteando.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Registrar Application
            builder.Services.AddSecurityApplication();
            builder.Services.AddMobilityApplication();

            // Registrar Infrastructure
            builder.Services.AddSecurityInfrastructure(builder.Configuration);
            builder.Services.AddMobilityInfrastructure(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // Controllers + Swagger
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
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
                };

                // Política para endpoints de auth: 5 requests por 5 min por IP
                options.AddFixedWindowLimiter("auth", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(5);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                // Política para forgot-password / send-verification: 3 por hora
                options.AddFixedWindowLimiter("sensitive", opt =>
                {
                    opt.PermitLimit = 3;
                    opt.Window = TimeSpan.FromHours(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                // Política general API: 100 por minuto
                options.AddFixedWindowLimiter("api", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    policy
                        .WithOrigins(
                        "http://localhost:3000",
                        "http://localhost:3001",
                        "http://localhost:3007",
                        "http://localhost:3002",
                        "http://10.3.235.158:3007")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
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

            // Detrás del proxy inverso (nginx): reconstruye la IP real del cliente
            // a partir de X-Forwarded-For para que el rate limiting sea por usuario
            // y no por la IP compartida del contenedor nginx.
            var forwardedHeaders = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            // Dentro de la red Docker el único proxy es nginx:
            // se limpia la lista de proxies confiables (por defecto solo loopback).
            forwardedHeaders.KnownNetworks.Clear();
            forwardedHeaders.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaders);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseCors("ReactPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();



            app.Run();
        }
    }
}