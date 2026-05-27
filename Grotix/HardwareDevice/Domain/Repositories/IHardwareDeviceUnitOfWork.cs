namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface IHardwareDeviceUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
