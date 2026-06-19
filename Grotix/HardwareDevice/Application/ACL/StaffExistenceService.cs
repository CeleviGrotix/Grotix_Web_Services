using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.HardwareDevice.Application.ACL;

public sealed class StaffExistenceService(IStaffRepository staffRepository) : IStaffExistenceService
{
    public async Task<bool> ExistsAsync(int staffId, CancellationToken cancellationToken = default) =>
        await staffRepository.GetByIdAsync(staffId) != null;
}
