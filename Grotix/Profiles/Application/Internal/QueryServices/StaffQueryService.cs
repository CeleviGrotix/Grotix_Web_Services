using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public class StaffQueryService(
    IStaffRepository staffRepository,
    IUserRepository userRepository) : IStaffQueryService
{
    public async Task<IReadOnlyList<Staff>> GetAllAsync()
    {
        var list = await staffRepository.ListAsync();
        return list.OrderBy(s => s.Id).ToList();
    }

    public async Task<Staff?> GetByIdAsync(int staffId) =>
        await staffRepository.GetByIdAsync(staffId);

    public async Task<Staff?> GetByIdentityIdAsync(int identityId)
    {
        // Paso 1: IdentityId → User
        var user = await userRepository.GetByIdentityIdAsync(identityId);
        if (user is null) return null;

        // Paso 2: UserId → Staff
        return await staffRepository.GetByUserIdAsync(user.Id);
    }
}