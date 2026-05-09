namespace GrotixBackend.BuildingBlocks.RabbitMq;

public sealed class NoOpRabbitMqPublisher : IRabbitMqPublisher
{
    public void Publish(string routingKey, ReadOnlyMemory<byte> body, string contentType = "application/json")
    {
    }
}
