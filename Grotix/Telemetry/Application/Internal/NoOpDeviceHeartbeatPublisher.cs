using GrotixBackend.Contracts.Integration.Hardware;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class NoOpDeviceHeartbeatPublisher : IDeviceHeartbeatPublisher
{
    public void Publish(DeviceHeartbeatIntegrationEvent heartbeat) { }
}
