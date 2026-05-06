namespace GrotixBackend.CultivationArea.Interfaces.REST.Resources;

public record ZoneResource(
    int Id,
    int FarmId,
    int CropId,
    string? CurrentPhase,
    DateTime? PhaseStartDate,
    string? ImageUrl,
    double Latitude,
    double Longitude);
