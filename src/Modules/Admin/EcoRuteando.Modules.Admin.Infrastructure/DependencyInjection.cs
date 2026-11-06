using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Modules.Admin.Infrastructure.Persistence;
using EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRuteando.Modules.Admin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AdminDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "admin");
                npgsqlOptions.MapEnum<EcoRuteando.Modules.Admin.Domain.Enums.SupportTicketStatus>("support_status", "admin");
            }));

        services.AddScoped<IAdminUnitOfWork>(sp =>
            sp.GetRequiredService<AdminDbContext>());

        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
        services.AddScoped<ITransportFactorRepository, TransportFactorRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICarbonCompensationRepository, CarbonCompensationRepository>();
        services.AddScoped<IDailyStatisticRepository, DailyStatisticRepository>();
        services.AddScoped<IAdminExportRepository, AdminExportRepository>();

        return services;
    }
}
