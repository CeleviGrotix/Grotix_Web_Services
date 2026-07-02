using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Irrigation;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.Profiles.Infrastructure.Integration;

/// <summary>Crea notificaciones in-app cuando inicia un ciclo de riego.</summary>
public sealed class RabbitMqIrrigationStartedNotificationConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqIrrigationStartedNotificationConsumerHostedService> logger) : BackgroundService
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
            logger.LogWarning("RabbitMQ irrigation started consumer not started.");
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
                var evt = JsonSerializer.Deserialize<IrrigationStartedIntegrationEvent>(json);
                if (evt != null)
                    await NotifyAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing irrigation started notification");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.ProfilesIrrigationStartedQueueName, autoAck: false, consumer);
        logger.LogInformation(
            "Consuming irrigation started from queue={Queue}",
            _options.ProfilesIrrigationStartedQueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task NotifyAsync(IrrigationStartedIntegrationEvent evt, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var zoneMembers = scope.ServiceProvider.GetRequiredService<IZoneMemberRepository>();
        var notifications = scope.ServiceProvider.GetRequiredService<IUserNotificationCommandService>();

        var members = await zoneMembers.ListByZoneIdAsync(evt.ZoneId);
        if (members.Count == 0)
            return;

        var title = $"Riego iniciado — zona {evt.ZoneId}";
        var message =
            $"El ciclo #{evt.CycleId} ha comenzado. Volumen: {evt.VolumeLiters:F1} L, duración: {evt.DurationMinutes} min.";

        foreach (var member in members)
        {
            try
            {
                await notifications.CreateAsync(member.UserId, title, message, "info", cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to notify user {UserId} — irrigation started", member.UserId);
            }
        }

        logger.LogInformation(
            "Irrigation started notifications sent: zone={ZoneId} members={Count}",
            evt.ZoneId,
            members.Count);
    }
}
