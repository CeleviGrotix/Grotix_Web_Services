namespace GrotixBackend.HardwareDevice.Domain.Model.Entities;

/// <summary>Registro de mantenimiento de dispositivo (tabla <c>maintenance_log</c>).</summary>
public class MaintenanceLog
{
    public int Id { get; private set; }
    public int DeviceId { get; private set; }
    public int UserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string StatusAfter { get; private set; } = null!;
    public DateTime Timestamp { get; private set; }

    protected MaintenanceLog() { }

    public MaintenanceLog(int deviceId, int userId, string action, string statusAfter, DateTime? timestamp = null)
    {
        if (deviceId <= 0)
            throw new ArgumentException("DeviceId inválido.");
        if (userId <= 0)
            throw new ArgumentException("UserId inválido.");
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action requerido.");
        if (string.IsNullOrWhiteSpace(statusAfter))
            throw new ArgumentException("StatusAfter requerido.");

        DeviceId = deviceId;
        UserId = userId;
        Action = action.Trim();
        StatusAfter = statusAfter.Trim().ToUpperInvariant();
        Timestamp = timestamp ?? DateTime.UtcNow;
    }
}
