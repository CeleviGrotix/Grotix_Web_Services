using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

/// <summary>
/// Consumidor activo de cambios de estado de dispositivos de hardware.
/// </summary>
public sealed class RabbitMqDeviceStatusChangedConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<RabbitMqDeviceStatusChangedConsumerHostedService> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            var connection = connectionHolder.TryGetConnection();
            if (connection == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            using var channel = connection.CreateModel();
            channel.QueueDeclare(_options.DeviceStatusChangedQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.Span);
                    var evt = JsonSerializer.Deserialize<DeviceStatusChangedIntegrationEvent>(json);
                    if (evt != null)
                    {
                        logger.LogInformation(
                            "hardware.device.status.changed -> DeviceId={DeviceId}, {OldStatus}->{NewStatus}, Timestamp={Timestamp}",
                            evt.DeviceId,
                            evt.OldStatus,
                            evt.NewStatus,
                            evt.Timestamp);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error handling hardware.device.status.changed");
                }
                finally
                {
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
            };

            channel.BasicConsume(_options.DeviceStatusChangedQueueName, autoAck: false, consumer);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}

