using EcoRuteando.Modules.Integrations.Domain.Repositories;
using EcoRuteando.Modules.Integrations.Infrastructure.Persistence;
using EcoRuteando.Modules.Integrations.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRuteando.Modules.Integrations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<IntegrationsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integrations");
                npgsqlOptions.UseNetTopologySuite();
            }));

        services.AddScoped<IAirQualityDataRepository, AirQualityDataRepository>();
        services.AddScoped<IWeatherDataRepository, WeatherDataRepository>();
        services.AddScoped<IApiCacheRepository, ApiCacheRepository>();

        return services;
    }
}
