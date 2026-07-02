using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IDeviceOfflinePublisher
{
    void Publish(Microcontroller device);
}
