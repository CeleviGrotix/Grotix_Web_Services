namespace GrotixBackend.HardwareDevice.Domain.Model.Entities;

/// <summary>Mantenimiento técnico formal (tabla <c>technical_maintenance</c>).</summary>
public class TechnicalMaintenance
{
    public int Id { get; private set; }
    public int StaffId { get; private set; }
    public int DeviceId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public string? Results { get; private set; }

    protected TechnicalMaintenance() { }

    public TechnicalMaintenance(
        int staffId,
        int deviceId,
        string type,
        string description,
        DateTime? date = null,
        string? results = null)
    {
        if (staffId <= 0)
            throw new ArgumentException("StaffId inválido.");
        if (deviceId <= 0)
            throw new ArgumentException("DeviceId inválido.");
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type requerido.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description requerido.");

        StaffId = staffId;
        DeviceId = deviceId;
        Type = type.Trim();
        Description = description.Trim();
        Date = date ?? DateTime.UtcNow;
        Results = string.IsNullOrWhiteSpace(results) ? null : results.Trim();
    }

    public void SetResults(string? results) =>
        Results = string.IsNullOrWhiteSpace(results) ? null : results.Trim();
}
