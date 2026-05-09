using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.BuildingBlocks.RabbitMq;

public static class RabbitMqServiceCollectionExtensions
{
    /// <summary>Registra conexión lazy, topología al arranque y publicador (o no-op si RabbitMq:Enabled=false).</summary>
    public static IServiceCollection AddGrotixRabbitMqPublisher(this IServiceCollection services, IConfiguration configuration)
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
        services.AddHostedService<RabbitMqTopologyInitializer>();
        return services;
    }
}
