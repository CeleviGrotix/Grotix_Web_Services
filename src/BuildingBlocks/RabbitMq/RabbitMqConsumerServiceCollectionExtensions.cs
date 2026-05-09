using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.BuildingBlocks.RabbitMq;

public static class RabbitMqConsumerServiceCollectionExtensions
{
    /// <summary>Conexión lazy, topología y consumidor de <c>user.registered</c> (omitido si RabbitMq:Enabled=false).</summary>
    public static IServiceCollection AddGrotixRabbitMqConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        var enabled = configuration.GetValue("RabbitMq:Enabled", true);
        if (!enabled)
            return services;

        services.AddSingleton<RabbitMqConnectionHolder>();
        services.AddHostedService<RabbitMqTopologyInitializer>();
        services.AddHostedService<RabbitMqUserRegisteredConsumerHostedService>();
        return services;
    }
}
