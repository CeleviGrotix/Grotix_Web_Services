namespace GrotixBackend.BuildingBlocks.RabbitMq;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; set; } = true;

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    /// <summary>Exchange tipo topic para eventos de integración.</summary>
    public string ExchangeName { get; set; } = "grotix.events";

    /// <summary>Cola que consume CultivationArea para user.registered.</summary>
    public string UserRegisteredQueueName { get; set; } = "cultivation.user.registered";

    public string UserRegisteredRoutingKey { get; set; } = "user.registered";
}
