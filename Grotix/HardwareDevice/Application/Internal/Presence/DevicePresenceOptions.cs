namespace GrotixBackend.HardwareDevice.Application.Internal.Presence;

public sealed class DevicePresenceOptions
{
    public const string SectionName = "Hardware:DevicePresence";

    public bool Enabled { get; set; } = true;

    /// <summary>Minutos sin telemetría antes de marcar OFFLINE.</summary>
    public int OfflineAfterMinutes { get; set; } = 60;

    /// <summary>Intervalo del job que revisa dispositivos inactivos.</summary>
    public int CheckIntervalMinutes { get; set; } = 5;
}
