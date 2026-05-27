using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GrotixBackend.BuildingBlocks.RabbitMq;

/// <summary>Declara exchange, cola y binding de forma idempotente al arranque.</summary>
public sealed class RabbitMqTopologyInitializer(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<RabbitMqTopologyInitializer> logger) : IHostedService
{
    private readonly RabbitMqOptions _options = options.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return Task.CompletedTask;

        var connection = connectionHolder.TryGetConnection();
        if (connection == null)
            return Task.CompletedTask;

        try
        {
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
            channel.QueueDeclare(_options.UserRegisteredQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_options.UserRegisteredQueueName, _options.ExchangeName, _options.UserRegisteredRoutingKey);

            channel.QueueDeclare(_options.TelemetryReceivedQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_options.TelemetryReceivedQueueName, _options.ExchangeName, _options.TelemetryReceivedRoutingKey);

            logger.LogInformation(
                "RabbitMQ topology OK: exchange={Exchange}, queues={UserQueue},{TelemetryQueue}",
                _options.ExchangeName,
                _options.UserRegisteredQueueName,
                _options.TelemetryReceivedQueueName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ topology declare skipped.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
