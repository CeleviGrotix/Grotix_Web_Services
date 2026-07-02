using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.CultivationArea.Domain.Model.ValueObjects;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Application.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

/// <summary>
/// MVP: ante alerta de humedad baja, inicia riego automático si no hay ciclo activo en la zona.
/// </summary>
public sealed class RabbitMqAlertTriggeredConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqAlertTriggeredConsumerHostedService> logger) : BackgroundService
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
            logger.LogWarning("RabbitMQ irrigation alert consumer not started.");
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
                if (evt != null && ShouldAutoIrrigate(evt))
                {
                    await using var scope = services.CreateAsyncScope();
                    var contextService = scope.ServiceProvider.GetRequiredService<IIrrigationContextService>();
                    var context = await contextService.GetZoneContextAsync(evt.ZoneId, stoppingToken);
                    if (context == null || !IrrigationModes.IsAutomatic(context.IrrigationMode))
                    {
                        logger.LogDebug(
                            "Skipping auto-irrigation for zone {ZoneId}: irrigation mode is {Mode}.",
                            evt.ZoneId,
                            context?.IrrigationMode ?? "UNKNOWN");
                        return;
                    }

                    var command = scope.ServiceProvider.GetRequiredService<IIrrigationCommandService>();
                    try
                    {
                        await command.StartManualAsync(evt.ZoneId, volumeLiters: null, durationMinutes: null, stoppingToken);
                        logger.LogInformation(
                            "Auto-irrigation started for zone {ZoneId} after {SensorType} alert (value={Value}, threshold={Threshold})",
                            evt.ZoneId,
                            evt.SensorType,
                            evt.Value,
                            evt.Threshold);
                    }
                    catch (InvalidOperationException)
                    {
                        // ciclo activo existente
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing alert.triggered for irrigation");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.AlertTriggeredQueueName, autoAck: false, consumer);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private static bool ShouldAutoIrrigate(AlertTriggeredIntegrationEvent evt)
    {
        if (!string.Equals(evt.BreachDirection, "BELOW_MIN", StringComparison.OrdinalIgnoreCase))
            return false;

        var type = evt.SensorType.ToUpperInvariant();
        return type.Contains("MOIST", StringComparison.Ordinal) ||
               type.Contains("HUMID", StringComparison.Ordinal);
    }
}
