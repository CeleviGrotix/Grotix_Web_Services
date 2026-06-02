namespace GrotixBackend.CultivationArea.Domain.Model.Aggregates;

/// <summary>Zona de cultivo dentro de una granja (tabla <c>zone</c>).</summary>
public class Zone
{
    public int Id { get; private set; }
    public int FarmId { get; private set; }
    public int CropId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? CurrentPhase { get; private set; }
    public DateTime? PhaseStartDate { get; private set; }
    public string? ImageUrl { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    protected Zone() { }

    public Zone(
        int farmId,
        int cropId,
        string name,
        double latitude,
        double longitude,
        string? currentPhase = null,
        DateTime? phaseStartDate = null,
        string? imageUrl = null)
    {
        if (farmId <= 0 || cropId <= 0)
            throw new ArgumentException("FarmId y CropId deben ser válidos.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la zona no puede estar vacío.");
        FarmId = farmId;
        CropId = cropId;
        Name = name.Trim();
        Latitude = latitude;
        Longitude = longitude;
        CurrentPhase = string.IsNullOrWhiteSpace(currentPhase) ? null : currentPhase.Trim();
        PhaseStartDate = phaseStartDate;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la zona no puede estar vacío.");
        Name = name.Trim();
    }

    public void ReassignCrop(int cropId)
    {
        if (cropId <= 0)
            throw new ArgumentException("CropId inválido.");
        CropId = cropId;
    }

    public void UpdatePhase(string? currentPhase, DateTime? phaseStartDate)
    {
        CurrentPhase = string.IsNullOrWhiteSpace(currentPhase) ? null : currentPhase.Trim();
        PhaseStartDate = phaseStartDate;
    }

    public void UpdateCoordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public void UpdateImageUrl(string? imageUrl) =>
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
}
