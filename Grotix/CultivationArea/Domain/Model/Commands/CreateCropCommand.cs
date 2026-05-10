namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record CreateCropCommand(
    string CommonName,
    string ScientificName,
    double OptimalTemperature,
    double OptimalHumidity,
    double OptimalLight,
    int MaxStressTime,
    string? ImageUrl = null);
