namespace GrotixBackend.CultivationArea.Interfaces.REST.Resources;

public record CropResource(
    int Id,
    string CommonName,
    string ScientificName,
    double OptimalTemperature,
    double OptimalHumidity,
    double OptimalLight,
    int MaxStressTime,
    string? ImageUrl);
