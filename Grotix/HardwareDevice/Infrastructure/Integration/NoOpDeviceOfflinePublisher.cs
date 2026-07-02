using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

public sealed class NoOpDeviceOfflinePublisher : IDeviceOfflinePublisher
{
    public void Publish(Microcontroller device) { }
}
