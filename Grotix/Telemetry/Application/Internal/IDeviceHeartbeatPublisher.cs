using GrotixBackend.Contracts.Integration.Hardware;

namespace GrotixBackend.Telemetry.Application.Internal;

public interface IDeviceHeartbeatPublisher
{
    void Publish(DeviceHeartbeatIntegrationEvent heartbeat);
}
