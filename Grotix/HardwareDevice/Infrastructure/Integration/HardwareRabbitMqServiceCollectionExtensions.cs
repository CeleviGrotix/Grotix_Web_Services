using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Infrastructure.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public static class HardwareRabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixHardwareRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        var enabled = configuration.GetValue("RabbitMq:Enabled", true);
        if (!enabled)
        {
            services.AddSingleton<IRabbitMqPublisher, NoOpRabbitMqPublisher>();
            return services;
        }

        services.AddSingleton<RabbitMqConnectionHolder>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddHostedService<HardwareRabbitMqTopologyInitializer>();
        services.AddScoped<IActuatorCommandOrchestrator, ActuatorCommandOrchestrator>();
        services.AddHostedService<RabbitMqActuatorCommandConsumerHostedService>();
        return services;
    }
}

