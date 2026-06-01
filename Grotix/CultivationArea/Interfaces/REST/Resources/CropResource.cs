namespace GrotixBackend.CultivationArea.Interfaces.REST.Resources;

public record CropResource(
    int Id,
    string CommonName,
    string ScientificName,
    double OptimalTemperature,
    double OptimalHumidityAir,
    double OptimalHumiditySoil,
    double OptimalLight,
    int MaxStressTime,
    string? ImageUrl);
