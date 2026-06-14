namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record CreateZoneCommand(
    int FarmId,
    int CropId,
    string Name,
    double Latitude,
    double Longitude,
    string? CurrentPhase,
    DateTime? PhaseStartDate,
    string? ImageUrl,
    string? IrrigationMode = null);
