using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Queries;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public interface IContractQueryService
{
    Task<Contract?> Handle(GetContractByIdQuery query);
    Task<IReadOnlyList<Contract>> ListAllAsync();

    Task<IReadOnlyList<Contract>> ListByAssociationAsync(int associationId);
}
