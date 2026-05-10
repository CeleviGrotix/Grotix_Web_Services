using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

public interface IAssociationInviteRepository : IAsyncRepository<AssociationInvite>
{
    Task<AssociationInvite?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Invitación pendiente misma asociación y correo (correo ya normalizado VO).</summary>
    Task<bool> HasPendingInviteForEmailAsync(int associationId, string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>Marca invitación usada si sigue libre (atomicidad).</summary>
    Task<bool> TryMarkUsedAsync(int inviteId, CancellationToken cancellationToken = default);
}
