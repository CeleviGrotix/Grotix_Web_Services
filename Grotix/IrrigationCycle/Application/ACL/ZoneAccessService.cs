using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;

namespace GrotixBackend.IrrigationCycle.Application.ACL;

public sealed class ZoneAccessService(
    IZoneQueryService zoneQueryService,
    IFarmQueryService farmQueryService) : IZoneAccessService
{
    public async Task<bool> ZoneExistsAsync(int zoneId)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        return zone != null;
    }

    public async Task<bool> CanAccessZoneAsync(int zoneId, bool isAdmin, int? profileUserId)
    {
        if (isAdmin)
            return await ZoneExistsAsync(zoneId);

        if (!profileUserId.HasValue)
            return false;

        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null)
            return false;

        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(zone.FarmId));
        return farm != null && farm.UserId == profileUserId.Value;
    }
}
