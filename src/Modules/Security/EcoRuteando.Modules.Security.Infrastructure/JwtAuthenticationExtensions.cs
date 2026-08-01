using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Infrastructure
{
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
             OnMessageReceived = context =>
             {
                 Console.WriteLine("========== TOKEN RECIBIDO ==========");
                 Console.WriteLine(context.Token ?? "TOKEN NULL");
                 return Task.CompletedTask;
             },

             OnAuthenticationFailed = context =>
             {
                 Console.WriteLine("========== AUTH FAILED ==========");
                 Console.WriteLine(context.Exception.ToString());
                 return Task.CompletedTask;
             },

             OnTokenValidated = context =>
             {
                 Console.WriteLine("========== TOKEN VALIDATED ==========");
                 return Task.CompletedTask;
             },

             OnChallenge = context =>
             {
                 Console.WriteLine("========== CHALLENGE ==========");
                 return Task.CompletedTask;
             }
         };
     });
            return services;
        }

    }
}
