using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

public class ContractRepository(AppDbContext context)
    : BaseRepository<Contract>(context), IContractRepository
{
    public override async Task<IEnumerable<Contract>> ListAsync() =>
        await Context.Set<Contract>().OrderByDescending(c => c.StartDate).ToListAsync();
}
