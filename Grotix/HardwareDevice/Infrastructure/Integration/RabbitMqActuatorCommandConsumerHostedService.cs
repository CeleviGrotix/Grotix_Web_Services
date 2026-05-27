using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

/// <summary>
/// Consume comandos de actuador desde RabbitMQ y refleja el estado en Hardware DB.
/// </summary>
public sealed class RabbitMqActuatorCommandConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqActuatorCommandConsumerHostedService> logger) : BackgroundService
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

            IModel? channel = null;
            try
            {
                channel = connection.CreateModel();
                channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
                channel.QueueDeclare(_options.ActuatorCommandQueueName, durable: true, exclusive: false, autoDelete: false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, ea) =>
                {
                    var processed = await ProcessAsync(ea.Body, stoppingToken);
                    if (processed)
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                    else
                        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                };

                var consumerTag = channel.BasicConsume(
                    queue: _options.ActuatorCommandQueueName,
                    autoAck: false,
                    consumer: consumer);

                logger.LogInformation(
                    "Consuming actuator commands from queue={Queue}, consumerTag={ConsumerTag}",
                    _options.ActuatorCommandQueueName,
                    consumerTag);

                while (!stoppingToken.IsCancellationRequested && channel.IsOpen)
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Actuator command consumer loop failed, retrying.");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
            finally
            {
                try { channel?.Close(); } catch { }
                try { channel?.Dispose(); } catch { }
            }
        }
    }

    private async Task<bool> ProcessAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        ActuatorCommandIntegrationEvent? evt;
        try
        {
            var json = Encoding.UTF8.GetString(body.Span);
            evt = JsonSerializer.Deserialize<ActuatorCommandIntegrationEvent>(json);
            if (evt == null)
                return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid actuator command payload.");
            return false;
        }

        try
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<HardwareDeviceDbContext>();
            var telemetryDb = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
            var actionQueueRepository = scope.ServiceProvider.GetRequiredService<IActionQueueRepository>();
            var actuatorLogRepository = scope.ServiceProvider.GetRequiredService<IActuatorLogRepository>();

            var actuator = await db.Actuators
                .FirstOrDefaultAsync(a => a.Id == evt.ActuatorId, cancellationToken);

            if (actuator == null)
            {
                logger.LogWarning(
                    "Actuator not found for command: actuatorId={ActuatorId}, zoneId={ZoneId}",
                    evt.ActuatorId,
                    evt.ZoneId);
                return true;
            }

            var normalizedCommand = evt.Command.Trim().ToUpperInvariant();
            if (string.Equals(normalizedCommand, "OPEN", StringComparison.Ordinal))
                actuator.SetOpen();
            else if (string.Equals(normalizedCommand, "CLOSE", StringComparison.Ordinal))
                actuator.SetClosed();
            else
            {
                logger.LogWarning("Unsupported actuator command {Command}", evt.Command);
                return true;
            }

            var queueItem = await actionQueueRepository.FindLatestPendingByActuatorAsync(
                actuator.Id,
                normalizedCommand,
                cancellationToken);
            queueItem?.MarkCompleted();

            await actuatorLogRepository.AddAsync(
                new ActuatorLogEntry(actuator.Id, normalizedCommand, evt.RequestedAtUtc),
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await telemetryDb.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Actuator command applied: actuatorId={ActuatorId}, command={Command}, zoneId={ZoneId}",
                actuator.Id,
                evt.Command,
                evt.ZoneId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying actuator command.");
            return false;
        }
    }
}

