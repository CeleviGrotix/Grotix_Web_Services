namespace GrotixBackend.Contracts.Integration.Hardware;

/// <summary>
/// Comando para accionar un actuador físico (valvula/bomba) desde Irrigation.
/// </summary>
public sealed record ActuatorCommandIntegrationEvent(
    int ZoneId,
    int ActuatorId,
    int MicrocontrollerId,
    string ActuatorType,
    int Pin,
    string Command,
    DateTime RequestedAtUtc);

