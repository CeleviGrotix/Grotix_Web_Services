using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class NoOpDeviceStatusChangedPublisher : IDeviceStatusChangedPublisher
{
    public void Publish(Microcontroller device, string oldStatus, string newStatus) { }
}
