using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Hardware;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.Profiles.Infrastructure.Integration;

/// <summary>Crea notificaciones in-app cuando un dispositivo se pone OFFLINE.</summary>
public sealed class RabbitMqDeviceOfflineNotificationConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqDeviceOfflineNotificationConsumerHostedService> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        await Task.Yield();

        var connection = connectionHolder.EnsureConnected();
        if (connection == null)
        {
            logger.LogWarning("RabbitMQ device offline consumer not started.");
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
                var evt = JsonSerializer.Deserialize<DeviceOfflineIntegrationEvent>(json);
                if (evt != null)
                    await NotifyAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing device offline notification");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.ProfilesDeviceOfflineQueueName, autoAck: false, consumer);
        logger.LogInformation(
            "Consuming device offline from queue={Queue}",
            _options.ProfilesDeviceOfflineQueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task NotifyAsync(DeviceOfflineIntegrationEvent evt, CancellationToken cancellationToken)
    {
        if (!evt.ZoneId.HasValue)
            return;

        await using var scope = services.CreateAsyncScope();
        var zoneMembers = scope.ServiceProvider.GetRequiredService<IZoneMemberRepository>();
        var notifications = scope.ServiceProvider.GetRequiredService<IUserNotificationCommandService>();

        var title = $"Dispositivo #{evt.DeviceId} desconectado";
        var message =
            $"El microcontrolador #{evt.DeviceId} de la zona {evt.ZoneId} se ha puesto OFFLINE. Verifica la conexión.";

        var members = await zoneMembers.ListByZoneIdAsync(evt.ZoneId.Value);
        if (members.Count == 0)
            return;

        foreach (var member in members)
        {
            try
            {
                await notifications.CreateAsync(member.UserId, title, message, "alert", cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to notify user {UserId} — device offline", member.UserId);
            }
        }

        logger.LogInformation(
            "Device offline notifications sent: device={DeviceId} zone={ZoneId} members={Count}",
            evt.DeviceId,
            evt.ZoneId,
            members.Count);
    }
}
