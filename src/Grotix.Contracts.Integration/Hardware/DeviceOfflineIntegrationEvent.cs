namespace GrotixBackend.Contracts.Integration.Hardware;

/// <summary>Dispositivo marcado OFFLINE por falta de telemetría.</summary>
public sealed record DeviceOfflineIntegrationEvent(
    int DeviceId,
    int? ZoneId,
    DateTime Timestamp);
