using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;

namespace GrotixBackend.HardwareDevice.Infrastructure.Repositories;

public sealed class HardwareDeviceUnitOfWork(HardwareDeviceDbContext context) : IHardwareDeviceUnitOfWork
{
    public Task CompleteAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
