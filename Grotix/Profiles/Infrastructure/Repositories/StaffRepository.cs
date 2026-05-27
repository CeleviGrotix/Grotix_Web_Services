using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

public class StaffRepository(ProfilesDbContext context)
    : BaseRepository<Staff>(context), IStaffRepository
{
    public async Task<Staff?> GetByUserIdAsync(int userId)
    {
        return await Context.Set<Staff>()
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
