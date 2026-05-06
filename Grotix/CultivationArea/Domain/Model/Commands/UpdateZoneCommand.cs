namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record UpdateZoneCommand(
    int ZoneId,
    int? CropId,
    double? Latitude,
    double? Longitude,
    string? CurrentPhase,
    DateTime? PhaseStartDate,
    string? ImageUrl);
