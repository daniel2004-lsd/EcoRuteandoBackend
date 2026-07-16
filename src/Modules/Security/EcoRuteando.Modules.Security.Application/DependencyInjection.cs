using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EcoRuteando.Modules.Security.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}