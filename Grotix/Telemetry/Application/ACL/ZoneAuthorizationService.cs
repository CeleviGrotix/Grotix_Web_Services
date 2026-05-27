using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;

namespace GrotixBackend.Telemetry.Application.ACL;

public sealed class ZoneAuthorizationService(
    IZoneQueryService zoneQueryService,
    IFarmQueryService farmQueryService) : IZoneAuthorizationService
{
    public async Task<bool> ZoneExistsAsync(int zoneId, CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        return zone != null;
    }

    public async Task<bool> CanAccessZoneAsync(
        int zoneId,
        bool isAdmin,
        int? profileUserId,
        CancellationToken cancellationToken = default)
    {
        if (isAdmin)
            return await ZoneExistsAsync(zoneId, cancellationToken);

        if (!profileUserId.HasValue)
            return false;

        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null)
            return false;

        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(zone.FarmId));
        return farm != null && farm.UserId == profileUserId.Value;
    }
}
