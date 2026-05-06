using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public class StaffQueryService(IStaffRepository staffRepository) : IStaffQueryService
{
    public async Task<IReadOnlyList<Staff>> GetAllAsync()
    {
        var list = await staffRepository.ListAsync();
        return list.OrderBy(s => s.Id).ToList();
    }

    public async Task<Staff?> GetByIdAsync(int staffId) =>
        await staffRepository.GetByIdAsync(staffId);
}
