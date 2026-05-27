using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Unidad de trabajo para escrituras del contexto de Profiles.
/// </summary>
public sealed class ProfilesUnitOfWork(ProfilesDbContext context) : IProfilesUnitOfWork
{
    public Task CompleteAsync() => context.SaveChangesAsync();
}
