namespace GrotixBackend.Telemetry.Application.ACL;

public interface IZoneAuthorizationService
{
    Task<bool> CanAccessZoneAsync(int zoneId, bool isAdmin, int? profileUserId, CancellationToken cancellationToken = default);

    Task<bool> ZoneExistsAsync(int zoneId, CancellationToken cancellationToken = default);
}
