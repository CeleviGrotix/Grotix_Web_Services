using GrotixBackend.BuildingBlocks.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.Profiles.Infrastructure.Integration;

public static class ProfilesRabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixProfilesRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        var enabled = configuration.GetValue("RabbitMq:Enabled", true);
        if (!enabled)
            return services;

        services.AddSingleton<RabbitMqConnectionHolder>();
        services.AddHostedService<ProfilesRabbitMqTopologyInitializer>();
        services.AddHostedService<RabbitMqAlertNotificationConsumerHostedService>();
        services.AddHostedService<RabbitMqIrrigationStartedNotificationConsumerHostedService>();
        services.AddHostedService<RabbitMqIrrigationCompletedNotificationConsumerHostedService>();
        services.AddHostedService<RabbitMqDeviceOfflineNotificationConsumerHostedService>();
        return services;
    }
}
