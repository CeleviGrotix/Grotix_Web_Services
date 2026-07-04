namespace GrotixBackend.BuildingBlocks.RabbitMq;



public interface IRabbitMqPublisher
{
    /// <summary>Publica si hay conexión activa. Devuelve false sin bloquear si RabbitMQ no está listo.</summary>
    bool TryPublish(string routingKey, ReadOnlyMemory<byte> body, string contentType = "application/json");
}
