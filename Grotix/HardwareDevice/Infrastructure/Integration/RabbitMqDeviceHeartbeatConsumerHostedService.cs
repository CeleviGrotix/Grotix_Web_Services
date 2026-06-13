using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.HardwareDevice.Application.Internal.Presence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class RabbitMqDeviceHeartbeatConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqDeviceHeartbeatConsumerHostedService> logger) : BackgroundService
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
            channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.Span);
                    var evt = JsonSerializer.Deserialize<DeviceHeartbeatIntegrationEvent>(json);
                    if (evt != null && evt.DeviceId > 0)
                    {
                        using var scope = scopeFactory.CreateScope();
                        var presenceService = scope.ServiceProvider.GetRequiredService<IDevicePresenceService>();
                        presenceService.RecordHeartbeatAsync(evt.DeviceId, evt.Timestamp).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error processing device.heartbeat");
                }
                finally
                {
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
            };

            channel.BasicConsume(_options.DeviceHeartbeatQueueName, autoAck: false, consumer);

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
