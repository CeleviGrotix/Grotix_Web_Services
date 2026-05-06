namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record CreateZoneCommand(
    int FarmId,
    int CropId,
    double Latitude,
    double Longitude,
    string? CurrentPhase,
    DateTime? PhaseStartDate,
    string? ImageUrl);
