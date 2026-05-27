using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Unidad de trabajo para escrituras del contexto de IAM.
/// </summary>
public sealed class IamUnitOfWork(IamDbContext context) : IIamUnitOfWork
{
    public Task CompleteAsync() => context.SaveChangesAsync();
}
