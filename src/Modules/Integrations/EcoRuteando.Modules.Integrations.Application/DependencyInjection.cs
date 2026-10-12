using Microsoft.Extensions.DependencyInjection;

namespace EcoRuteando.Modules.Integrations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrationsApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
