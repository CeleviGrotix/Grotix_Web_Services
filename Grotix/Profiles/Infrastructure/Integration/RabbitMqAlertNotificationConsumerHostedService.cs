using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.Profiles.Infrastructure.Integration;

/// <summary>Crea notificaciones in-app para miembros de la zona cuando se dispara una alerta.</summary>
public sealed class RabbitMqAlertNotificationConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqAlertNotificationConsumerHostedService> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        await Task.Yield();

        var connection = connectionHolder.TryGetConnection();
        if (connection == null)
        {
            logger.LogWarning("RabbitMQ profiles alert consumer not started.");
            return;
        }

        using var channel = connection.CreateModel();
        channel.BasicQos(0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<AlertTriggeredIntegrationEvent>(json);
                if (evt != null)
                    await NotifyZoneMembersAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing alert for notifications");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.AlertNotificationQueueName, autoAck: false, consumer);

        logger.LogInformation(
            "Consuming alert notifications from queue={Queue}",
            _options.AlertNotificationQueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task NotifyZoneMembersAsync(
        AlertTriggeredIntegrationEvent evt,
        CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var zoneMembers = scope.ServiceProvider.GetRequiredService<IZoneMemberRepository>();
        var notifications = scope.ServiceProvider.GetRequiredService<IUserNotificationCommandService>();

        var members = await zoneMembers.ListByZoneIdAsync(evt.ZoneId);
        if (members.Count == 0)
        {
            logger.LogDebug("No zone members to notify for zone {ZoneId}", evt.ZoneId);
            return;
        }

        var direction = evt.BreachDirection == "BELOW_MIN" ? "por debajo del mínimo" : "por encima del máximo";
        var title = $"Alerta en zona {evt.ZoneId}";
        var message =
            $"Sensor {evt.SensorType} (id {evt.SensorId}): lectura {evt.Value:F2} {direction}. " +
            $"Rango esperado [{evt.MinThreshold:F2}, {evt.MaxThreshold:F2}].";

        foreach (var member in members)
        {
            try
            {
                await notifications.CreateAsync(
                    member.UserId,
                    title,
                    message,
                    "alert",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to create alert notification for user {UserId} in zone {ZoneId}",
                    member.UserId,
                    evt.ZoneId);
            }
        }

        logger.LogInformation(
            "Alert notifications created for zone {ZoneId}: {Count} members",
            evt.ZoneId,
            members.Count);
    }
}
