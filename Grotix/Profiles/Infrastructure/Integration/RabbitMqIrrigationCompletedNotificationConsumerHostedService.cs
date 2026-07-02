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

/// <summary>Crea notificaciones in-app cuando finaliza un ciclo de riego.</summary>
public sealed class RabbitMqIrrigationCompletedNotificationConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqIrrigationCompletedNotificationConsumerHostedService> logger) : BackgroundService
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
            logger.LogWarning("RabbitMQ irrigation completed consumer not started.");
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
                var evt = JsonSerializer.Deserialize<IrrigationCompletedIntegrationEvent>(json);
                if (evt != null)
                    await NotifyAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing irrigation completed notification");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.ProfilesIrrigationCompletedQueueName, autoAck: false, consumer);
        logger.LogInformation(
            "Consuming irrigation completed from queue={Queue}",
            _options.ProfilesIrrigationCompletedQueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task NotifyAsync(IrrigationCompletedIntegrationEvent evt, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var zoneMembers = scope.ServiceProvider.GetRequiredService<IZoneMemberRepository>();
        var notifications = scope.ServiceProvider.GetRequiredService<IUserNotificationCommandService>();

        var members = await zoneMembers.ListByZoneIdAsync(evt.ZoneId);
        if (members.Count == 0)
            return;

        var isAborted = string.Equals(evt.Status, "ABORTED", StringComparison.OrdinalIgnoreCase);
        var title = isAborted
            ? $"Riego cancelado — zona {evt.ZoneId}"
            : $"Riego completado — zona {evt.ZoneId}";
        var message = isAborted
            ? $"El ciclo #{evt.CycleId} fue cancelado. Volumen aplicado: {evt.VolumeLiters:F1} L."
            : $"El ciclo #{evt.CycleId} finalizó correctamente. Volumen aplicado: {evt.VolumeLiters:F1} L.";
        var type = isAborted ? "warning" : "info";

        foreach (var member in members)
        {
            try
            {
                await notifications.CreateAsync(member.UserId, title, message, type, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to notify user {UserId} — irrigation completed", member.UserId);
            }
        }

        logger.LogInformation(
            "Irrigation completed notifications sent: zone={ZoneId} status={Status} members={Count}",
            evt.ZoneId,
            evt.Status,
            members.Count);
    }
}
