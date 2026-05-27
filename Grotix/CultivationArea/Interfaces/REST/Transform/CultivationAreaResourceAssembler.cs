using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Interfaces.REST.Resources;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Transform;

public static class CultivationAreaResourceAssembler
{
    public static FarmResource ToFarmResource(Farm f) =>
        new(f.Id, f.UserId, f.AssociationId, f.Name, f.Location);

    public static ZoneResource ToZoneResource(Zone z) =>
        new(z.Id, z.FarmId, z.CropId, z.CurrentPhase, z.PhaseStartDate, z.ImageUrl, z.Latitude, z.Longitude);

    public static CropResource ToCropResource(Crop c) =>
        new(c.Id, c.CommonName, c.ScientificName, c.OptimalTemperature, c.OptimalHumidity, c.OptimalLight,
            c.MaxStressTime, c.ImageUrl);
}
