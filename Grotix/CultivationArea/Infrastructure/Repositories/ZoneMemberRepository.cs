using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public sealed class ZoneMemberRepository(CultivationAreaDbContext context)
    : BaseRepository<ZoneMember>(context), IZoneMemberRepository
{
    public Task<bool> ExistsAsync(int zoneId, int userId) =>
        Context.Set<ZoneMember>().AnyAsync(m => m.ZoneId == zoneId && m.UserId == userId);

    public async Task<IReadOnlyList<ZoneMember>> ListByZoneIdAsync(int zoneId) =>
        await Context.Set<ZoneMember>()
            .Where(m => m.ZoneId == zoneId)
            .OrderBy(m => m.Id)
            .ToListAsync();

    public Task<ZoneMember?> GetByZoneAndUserAsync(int zoneId, int userId) =>
        Context.Set<ZoneMember>()
            .FirstOrDefaultAsync(m => m.ZoneId == zoneId && m.UserId == userId);

    public Task<bool> IsUserAssignedToZoneAsync(int zoneId, int userId) =>
        ExistsAsync(zoneId, userId);

    public async Task<IReadOnlyList<int>> ListAssignedZoneIdsForFarmAsync(int farmId, int userId)
    {
        return await Context.Set<ZoneMember>()
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Join(
                Context.Set<Zone>().AsNoTracking(),
                m => m.ZoneId,
                z => z.Id,
                (m, z) => new { m.ZoneId, z.FarmId })
            .Where(x => x.FarmId == farmId)
            .Select(x => x.ZoneId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<int>> ListFarmIdsForUserAsync(int userId)
    {
        return await Context.Set<ZoneMember>()
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Join(
                Context.Set<Zone>().AsNoTracking(),
                m => m.ZoneId,
                z => z.Id,
                (_, z) => z.FarmId)
            .Distinct()
            .ToListAsync();
    }
}
