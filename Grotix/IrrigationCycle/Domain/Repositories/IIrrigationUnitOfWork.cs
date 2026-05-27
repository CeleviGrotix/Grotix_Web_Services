namespace GrotixBackend.IrrigationCycle.Domain.Repositories;

public interface IIrrigationUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
