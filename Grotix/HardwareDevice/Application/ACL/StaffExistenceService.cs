using GrotixBackend.Profiles.Application.Internal.QueryServices;

namespace GrotixBackend.HardwareDevice.Application.ACL;

public sealed class StaffExistenceService(IStaffQueryService staffQueryService) : IStaffExistenceService
{
    public async Task<bool> ExistsAsync(int staffId, CancellationToken cancellationToken = default) =>
        await staffQueryService.GetByIdAsync(staffId) != null;
}
