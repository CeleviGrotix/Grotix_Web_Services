using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

public interface IAssociationInviteRepository : IAsyncRepository<AssociationInvite>
{
    Task<AssociationInvite?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Marca invitación usada si sigue libre (atomicidad).</summary>
    Task<bool> TryMarkUsedAsync(int inviteId, CancellationToken cancellationToken = default);
}
