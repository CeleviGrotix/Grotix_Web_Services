namespace GrotixBackend.IrrigationCycle.Application.ACL;

public interface IIrrigationContextService
{
    Task<IrrigationZoneContext?> GetZoneContextAsync(int zoneId, CancellationToken cancellationToken = default);
}

public sealed record IrrigationZoneContext(
    int ZoneId,
    int CropId,
    double OptimalHumiditySoil,
    double? CurrentHumiditySoilPercent,
    string IrrigationMode);
