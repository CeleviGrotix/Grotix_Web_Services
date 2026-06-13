namespace GrotixBackend.HardwareDevice.Application.Internal.Presence;

public interface IDevicePresenceService
{
    Task RecordHeartbeatAsync(int deviceId, DateTime timestamp, CancellationToken cancellationToken = default);

    Task MarkStaleDevicesOfflineAsync(CancellationToken cancellationToken = default);
}
