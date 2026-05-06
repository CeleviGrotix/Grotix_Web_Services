using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public class ContractQueryService(IContractRepository contractRepository) : IContractQueryService
{
    public async Task<Contract?> Handle(GetContractByIdQuery query) =>
        await contractRepository.GetByIdAsync(query.ContractId);

    public async Task<IReadOnlyList<Contract>> ListAllAsync()
    {
        var list = await contractRepository.ListAsync();
        return list.ToList();
    }
}
