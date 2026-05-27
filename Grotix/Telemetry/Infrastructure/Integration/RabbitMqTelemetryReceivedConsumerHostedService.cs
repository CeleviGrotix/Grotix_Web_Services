using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.Telemetry.Infrastructure.Integration;

public sealed class RabbitMqTelemetryReceivedConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    IServiceProvider services,
    ILogger<RabbitMqTelemetryReceivedConsumerHostedService> logger) : BackgroundService
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
            logger.LogWarning("RabbitMQ telemetry consumer not started (broker unavailable).");
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
                var evt = JsonSerializer.Deserialize<TelemetryReceivedIntegrationEvent>(json);
                if (evt != null)
                {
                    await using var scope = services.CreateAsyncScope();
                    var ingest = scope.ServiceProvider.GetRequiredService<ITelemetryIngestService>();
                    await ingest.IngestAsync(evt, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing telemetry.received");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.TelemetryReceivedQueueName, autoAck: false, consumer);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }
}
