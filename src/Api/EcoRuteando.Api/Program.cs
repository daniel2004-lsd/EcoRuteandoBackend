using EcoRuteando.Api.Middlewares;
using EcoRuteando.Modules.Security.Application;
using EcoRuteando.Modules.Security.Infrastructure;
using EcoRuteando.Modules.Security.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
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

            // Registrar Infrastructure
            builder.Services.AddSecurityInfrastructure(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
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

            app.MapControllers();



            app.Run();
        }
    }
}