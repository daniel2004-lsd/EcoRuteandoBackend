using EcoRuteando.Modules.Mobility.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EcoRuteando.Modules.Mobility.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMobilityApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
