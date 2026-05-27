using GrotixBackend.IrrigationCycle.Domain.Repositories;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Repositories;

public sealed class IrrigationUnitOfWork(IrrigationCycleDbContext context) : IIrrigationUnitOfWork
{
    public Task CompleteAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
