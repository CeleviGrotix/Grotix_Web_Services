namespace GrotixBackend.CultivationArea.Domain.Model.Aggregates;

/// <summary>Catálogo maestro de cultivos (tabla <c>crop</c>).</summary>
public class Crop
{
    public int Id { get; private set; }
    public string CommonName { get; private set; } = null!;
    public string ScientificName { get; private set; } = null!;
    public double OptimalTemperature { get; private set; }
    public double OptimalHumidity { get; private set; }
    public double OptimalLight { get; private set; }
    public int MaxStressTime { get; private set; }
    public string? ImageUrl { get; private set; }

    protected Crop() { }

    public Crop(
        string commonName,
        string scientificName,
        double optimalTemperature,
        double optimalHumidity,
        double optimalLight,
        int maxStressTime,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(commonName))
            throw new ArgumentException("CommonName no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(scientificName))
            throw new ArgumentException("ScientificName no puede estar vacío.");
        if (maxStressTime < 0)
            throw new ArgumentException("MaxStressTime debe ser >= 0.");

        CommonName = commonName.Trim();
        ScientificName = scientificName.Trim();
        OptimalTemperature = optimalTemperature;
        OptimalHumidity = optimalHumidity;
        OptimalLight = optimalLight;
        MaxStressTime = maxStressTime;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    public void UpdateBiologicalProfile(
        double optimalTemperature,
        double optimalHumidity,
        double optimalLight,
        int maxStressTime)
    {
        if (maxStressTime < 0)
            throw new ArgumentException("MaxStressTime debe ser >= 0.");
        OptimalTemperature = optimalTemperature;
        OptimalHumidity = optimalHumidity;
        OptimalLight = optimalLight;
        MaxStressTime = maxStressTime;
    }

    public void UpdateNames(string commonName, string scientificName)
    {
        if (string.IsNullOrWhiteSpace(commonName))
            throw new ArgumentException("CommonName no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(scientificName))
            throw new ArgumentException("ScientificName no puede estar vacío.");
        CommonName = commonName.Trim();
        ScientificName = scientificName.Trim();
    }

    public void UpdateImageUrl(string? imageUrl) =>
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
}
