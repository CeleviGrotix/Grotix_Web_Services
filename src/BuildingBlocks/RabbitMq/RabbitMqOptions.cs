namespace GrotixBackend.BuildingBlocks.RabbitMq;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// URI completa del broker (p.ej. amqps://user:pass@host/vhost).
    /// Cuando está configurada tiene prioridad sobre HostName/Port/UserName/Password/VirtualHost.
    /// </summary>
    public string? Uri { get; set; }

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

    public string TelemetryReceivedQueueName { get; set; } = "telemetry.received";

    public string TelemetryReceivedRoutingKey { get; set; } = "telemetry.received";

    public string AlertTriggeredRoutingKey { get; set; } = "alert.triggered";

  /// <summary>Cola consumida por IrrigationCycle para alertas críticas.</summary>
  public string AlertTriggeredQueueName { get; set; } = "irrigation.alert.triggered";

  /// <summary>Cola consumida por Profiles para notificaciones de alerta.</summary>
  public string AlertNotificationQueueName { get; set; } = "profiles.alert.triggered";

    public string IrrigationCompletedRoutingKey { get; set; } = "irrigation.completed";

    /// <summary>Routing key para comandos de actuadores del flujo de riego.</summary>
    public string ActuatorCommandRoutingKey { get; set; } = "hardware.actuator.command";

    /// <summary>Cola que consume Hardware (o bridge MQTT) para ejecutar comandos físicos.</summary>
    public string ActuatorCommandQueueName { get; set; } = "hardware.actuator.command";

    public string DeviceStatusChangedRoutingKey { get; set; } = "hardware.device.status.changed";

    /// <summary>Cola consumida por Irrigation para reaccionar a cambios de estado del hardware.</summary>
    public string DeviceStatusChangedQueueName { get; set; } = "irrigation.hardware.device.status.changed";

    /// <summary>Routing key publicada por Telemetry tras ingerir telemetría.</summary>
    public string DeviceHeartbeatRoutingKey { get; set; } = "device.heartbeat";

    /// <summary>Cola consumida por Hardware para marcar dispositivos ONLINE.</summary>
    public string DeviceHeartbeatQueueName { get; set; } = "hardware.device.heartbeat";
}
