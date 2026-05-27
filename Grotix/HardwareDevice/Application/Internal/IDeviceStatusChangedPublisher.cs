using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IDeviceStatusChangedPublisher
{
    void Publish(Microcontroller device, string oldStatus, string newStatus);
}

