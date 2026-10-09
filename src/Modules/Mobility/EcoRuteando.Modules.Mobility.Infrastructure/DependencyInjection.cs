using EcoRuteando.Modules.Mobility.Application.Abstractions.Export;
using EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Weather;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Modules.Mobility.Infrastructure.Export;
using EcoRuteando.Modules.Mobility.Infrastructure.GoogleMaps;
using EcoRuteando.Modules.Mobility.Infrastructure.Persistence;
using EcoRuteando.Modules.Mobility.Infrastructure.Weather;
using EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EcoRuteando.Modules.Mobility.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMobilityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<MobilityDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                o =>
                {
                    o.MigrationsAssembly("EcoRuteando.Modules.Mobility.Infrastructure");
                    o.MapEnum<TransportType>("transport_type", "mobility");
                    o.MapEnum<RouteStatus>("route_status", "mobility");
                    o.MapEnum<UsageSource>("usage_source", "mobility");
                    o.MapEnum<ReportStatus>("report_status", "community");
                    o.UseNetTopologySuite();
                });
        });

        services.AddScoped<IMobilityUnitOfWork>(sp =>
            sp.GetRequiredService<MobilityDbContext>());

        // Repositorios
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IPointOfInterestRepository, PointOfInterestRepository>();
        services.AddScoped<IRouteUsageRepository, RouteUsageRepository>();
        services.AddScoped<ITransportFactorRepository, TransportFactorRepository>();
        services.AddScoped<IFavoriteRouteRepository, FavoriteRouteRepository>();
        services.AddScoped<ISharedRouteRepository, SharedRouteRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IObstacleReportRepository, ObstacleReportRepository>();

        // Exportación de archivos (CU19): CSV, JSON y XLSX
        services.AddScoped<IExportFileService, ExportFileService>();

        // Google Maps
        services.Configure<GoogleMapsOptions>(
            configuration.GetSection("GoogleMaps"));

        services.AddHttpClient<IGoogleMapsService, GoogleMapsService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Google Weather API
        services.Configure<GoogleWeatherOptions>(options =>
        {
            options.ApiKey = configuration["GoogleMaps:ApiKey"] ?? string.Empty;
        });

        services.AddHttpClient<IWeatherService, WeatherService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
