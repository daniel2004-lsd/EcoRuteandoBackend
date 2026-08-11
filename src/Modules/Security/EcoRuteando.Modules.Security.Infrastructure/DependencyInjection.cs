using EcoRuteando.Modules.Security.Application.Abstractions.Email;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Authorization;
using EcoRuteando.Modules.Security.Infrastructure.Email;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using EcoRuteando.Shared.Abstractions;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace EcoRuteando.Modules.Security.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'.");


        services.Configure<JwtOptions>(
            configuration.GetSection("Jwt"));


        services.AddDbContext<SecurityDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                o =>
                {
                    o.MigrationsAssembly("EcoRuteando.Modules.Security.Infrastructure");
                });
        });

        services.AddScoped<IUnitOfWork>(sp =>
        sp.GetRequiredService<SecurityDbContext>());

        // Repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordRecoveryRepository, PasswordRecoveryRepository>();

        // Servicios
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IOtpProvider, OtpProvider>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}