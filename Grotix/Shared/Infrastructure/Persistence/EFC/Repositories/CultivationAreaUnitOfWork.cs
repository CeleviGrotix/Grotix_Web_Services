using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Unidad de trabajo para escrituras del contexto de CultivationArea.
/// </summary>
public sealed class CultivationAreaUnitOfWork(CultivationAreaDbContext context) : ICultivationAreaUnitOfWork
{
    public Task CompleteAsync() => context.SaveChangesAsync();
}
