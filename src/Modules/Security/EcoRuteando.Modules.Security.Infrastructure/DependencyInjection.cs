using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using EcoRuteando.Modules.Security.Infrastructure.Persistence.Repositories;
using EcoRuteando.Modules.Security.Infrastructure.Security;
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


        services.AddDbContext<SecurityDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                o =>
                {
                    o.MigrationsAssembly("EcoRuteando.Modules.Security.Infrastructure");
                });
        });

        // Repositorios
        services.AddScoped<IUserRepository, UserRepository>();

        // Servicios
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}