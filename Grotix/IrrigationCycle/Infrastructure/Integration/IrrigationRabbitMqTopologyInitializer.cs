using GrotixBackend.BuildingBlocks.RabbitMq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class IrrigationRabbitMqTopologyInitializer(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<IrrigationRabbitMqTopologyInitializer> logger) : IHostedService
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
            channel.QueueDeclare(
                _options.AlertTriggeredQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
            channel.QueueBind(
                _options.AlertTriggeredQueueName,
                _options.ExchangeName,
                _options.AlertTriggeredRoutingKey);

            channel.QueueDeclare(
                _options.DeviceStatusChangedQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
            channel.QueueBind(
                _options.DeviceStatusChangedQueueName,
                _options.ExchangeName,
                _options.DeviceStatusChangedRoutingKey);

            channel.QueueDeclare(
                _options.ActuatorCommandQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
            channel.QueueBind(
                _options.ActuatorCommandQueueName,
                _options.ExchangeName,
                _options.ActuatorCommandRoutingKey);

            logger.LogInformation(
                "Irrigation RabbitMQ topology OK: alertQueue={AlertQueue}, actuatorQueue={ActuatorQueue}, statusQueue={StatusQueue}",
                _options.AlertTriggeredQueueName,
                _options.ActuatorCommandQueueName,
                _options.DeviceStatusChangedQueueName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Irrigation RabbitMQ topology declare skipped.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
