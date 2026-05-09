using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GrotixBackend.BuildingBlocks.RabbitMq;

public sealed class RabbitMqPublisher(
    IOptions<RabbitMqOptions> options,
    RabbitMqConnectionHolder connectionHolder,
    ILogger<RabbitMqPublisher> logger) : IRabbitMqPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public void Publish(string routingKey, ReadOnlyMemory<byte> body, string contentType = "application/json")
    {
        var connection = connectionHolder.TryGetConnection();
        if (connection == null)
            return;

        try
        {
            using var channel = connection.CreateModel();
            var props = channel.CreateBasicProperties();
            props.ContentType = contentType;
            props.DeliveryMode = 2;

            channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                basicProperties: props,
                body: body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ publish failed for routing key {RoutingKey}", routingKey);
        }
    }
}
