namespace GrotixBackend.HardwareDevice.Application.ACL;

public interface IZoneAccessService
{
    Task<bool> ZoneExistsAsync(int zoneId);

    Task<bool> CanAccessZoneAsync(int zoneId, bool isAdmin, int? profileUserId);
}
