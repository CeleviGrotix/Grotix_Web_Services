using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.IrrigationCycle.Application.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public static class IrrigationRabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixIrrigationRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        var enabled = configuration.GetValue("RabbitMq:Enabled", true);
        if (!enabled)
        {
            services.AddSingleton<IRabbitMqPublisher, NoOpRabbitMqPublisher>();
            services.AddSingleton<IIrrigationCompletedPublisher, NoOpIrrigationCompletedPublisher>();
            return services;
        }

        services.AddSingleton<RabbitMqConnectionHolder>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddScoped<IIrrigationCompletedPublisher, RabbitMqIrrigationCompletedPublisher>();
        services.AddHostedService<IrrigationRabbitMqTopologyInitializer>();
        services.AddHostedService<RabbitMqAlertTriggeredConsumerHostedService>();
        services.AddHostedService<RabbitMqDeviceStatusChangedConsumerHostedService>();
        return services;
    }
}
