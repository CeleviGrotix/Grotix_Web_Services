namespace GrotixBackend.CultivationArea.Domain.Model.Commands;

public record CreateCropCommand(
    string CommonName,
    string ScientificName,
    double OptimalTemperature,
    double OptimalHumidityAir,
    double OptimalHumiditySoil,
    double OptimalLight,
    int MaxStressTime,
    string? ImageUrl = null);
