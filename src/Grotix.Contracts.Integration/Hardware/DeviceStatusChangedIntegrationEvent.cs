namespace GrotixBackend.Contracts.Integration.Hardware;

public sealed record DeviceStatusChangedIntegrationEvent(
    int DeviceId,
    string OldStatus,
    string NewStatus,
    DateTime Timestamp);
