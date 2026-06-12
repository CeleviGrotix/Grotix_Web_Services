namespace GrotixBackend.HardwareDevice.Application.ACL;

public interface IStaffExistenceService
{
    Task<bool> ExistsAsync(int staffId, CancellationToken cancellationToken = default);
}
