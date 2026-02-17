using CoffeeMachine.Application;
using CoffeeMachine.Application.Interfaces;
using CoffeeMachine.Infrastructure.External;
using CoffeeMachine.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeMachine.Infrastructure.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddLibraries();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddHttpClient<OpenWeatherMapClient>();
        services.AddHttpClient<IWeatherClient, OpenWeatherMapClient>();

        return services;
    }

    public static IServiceCollection AddLibraries(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

        return services;
    }
}
