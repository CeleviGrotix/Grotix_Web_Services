using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.Telemetry.Infrastructure.Integration;

public static class TelemetryRabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixTelemetryAlertPublisher(this IServiceCollection services)
    {
        services.AddScoped<IAlertPublisher, RabbitMqAlertPublisher>();
        return services;
    }

    public static IServiceCollection AddGrotixTelemetryRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        var enabled = configuration.GetValue("RabbitMq:Enabled", true);
        if (!enabled)
            return services;

        services.AddSingleton<RabbitMqConnectionHolder>();
        services.AddHostedService<RabbitMqTopologyInitializer>();
        services.AddHostedService<RabbitMqTelemetryReceivedConsumerHostedService>();
        return services;
    }
}
