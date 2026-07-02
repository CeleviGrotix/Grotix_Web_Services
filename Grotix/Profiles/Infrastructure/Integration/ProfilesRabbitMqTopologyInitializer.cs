using GrotixBackend.BuildingBlocks.RabbitMq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GrotixBackend.Profiles.Infrastructure.Integration;

public sealed class ProfilesRabbitMqTopologyInitializer(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<ProfilesRabbitMqTopologyInitializer> logger) : IHostedService
{
    private readonly RabbitMqOptions _options = options.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return Task.CompletedTask;

        var connection = connectionHolder.EnsureConnected();
        if (connection == null)
            return Task.CompletedTask;

        try
        {
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
            channel.QueueDeclare(_options.AlertNotificationQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(
                _options.AlertNotificationQueueName,
                _options.ExchangeName,
                _options.AlertTriggeredRoutingKey);

            logger.LogInformation(
                "Profiles RabbitMQ topology OK: alertNotificationQueue={Queue} -> {RoutingKey}",
                _options.AlertNotificationQueueName,
                _options.AlertTriggeredRoutingKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Profiles RabbitMQ topology declare skipped.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
