namespace GrotixBackend.BuildingBlocks.RabbitMq;

public interface IRabbitMqPublisher
{
    void Publish(string routingKey, ReadOnlyMemory<byte> body, string contentType = "application/json");
}
