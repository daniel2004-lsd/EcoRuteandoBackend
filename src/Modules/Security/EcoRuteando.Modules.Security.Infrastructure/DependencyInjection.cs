using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Security;
using EcoRuteando.Shared.Abstractions;
using EcoRuteando.Shared.Abstractions.Persistence;
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

        // Servicios
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}