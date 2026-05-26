using System.Text;
using System.Text.Json;
using GrotixBackend.Contracts.Integration.Users;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GrotixBackend.BuildingBlocks.RabbitMq;

/// <summary>Consumidor de ejemplo: registra en log el payload; sustituir por lógica de negocio.</summary>
public sealed class RabbitMqUserRegisteredConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<RabbitMqUserRegisteredConsumerHostedService> logger) : BackgroundService
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
            logger.LogWarning(
                "RabbitMQ consumer no iniciado (broker no disponible). CultivationArea sigue sin escuchar colas.");
            return;
        }

        using var channel = connection.CreateModel();
        channel.BasicQos(0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(json);
                logger.LogInformation(
                    "[RabbitMQ] user.registered -> IdentityId={IdentityId}, Email={Email}, raw={Raw}",
                    evt?.IdentityId,
                    evt?.Email,
                    json);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing user.registered");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
        };

        channel.BasicConsume(_options.UserRegisteredQueueName, autoAck: false, consumer);

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
