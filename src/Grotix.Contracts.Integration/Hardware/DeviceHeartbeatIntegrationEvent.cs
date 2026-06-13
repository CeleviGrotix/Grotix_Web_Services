namespace GrotixBackend.Contracts.Integration.Hardware;

/// <summary>Señal de actividad de un dispositivo tras recibir telemetría.</summary>
public sealed record DeviceHeartbeatIntegrationEvent(
    int DeviceId,
    DateTime Timestamp);
