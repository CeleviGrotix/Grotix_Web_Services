namespace GrotixBackend.BuildingBlocks.RabbitMq;

public sealed class NoOpRabbitMqPublisher : IRabbitMqPublisher
{
    public bool TryPublish(string routingKey, ReadOnlyMemory<byte> body, string contentType = "application/json") =>
        false;
}
