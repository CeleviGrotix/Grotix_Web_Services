using GrotixBackend.BuildingBlocks.RabbitMq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class HardwareRabbitMqTopologyInitializer(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<HardwareRabbitMqTopologyInitializer> logger) : IHostedService
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
            channel.QueueDeclare(_options.ActuatorCommandQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_options.ActuatorCommandQueueName, _options.ExchangeName, _options.ActuatorCommandRoutingKey);
            channel.QueueDeclare(_options.DeviceHeartbeatQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_options.DeviceHeartbeatQueueName, _options.ExchangeName, _options.DeviceHeartbeatRoutingKey);

            logger.LogInformation(
                "Hardware RabbitMQ topology OK: queue={Queue}, routingKey={RoutingKey}",
                _options.ActuatorCommandQueueName,
                _options.ActuatorCommandRoutingKey);
            logger.LogInformation(
                "Hardware RabbitMQ topology OK: queue={Queue}, routingKey={RoutingKey}",
                _options.DeviceHeartbeatQueueName,
                _options.DeviceHeartbeatRoutingKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Hardware RabbitMQ topology declare skipped.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

