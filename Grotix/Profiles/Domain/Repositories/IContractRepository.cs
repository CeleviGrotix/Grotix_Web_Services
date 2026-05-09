using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

public interface IContractRepository : IAsyncRepository<Contract>
{
    Task<IReadOnlyList<Contract>> ListByAssociationIdAsync(int associationId, CancellationToken cancellationToken = default);
}
