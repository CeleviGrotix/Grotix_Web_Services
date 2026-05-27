namespace GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;

public static class DeviceStatuses
{
    public const string Online = "ONLINE";
    public const string Offline = "OFFLINE";
    public const string Maintenance = "MAINTENANCE";
    public const string Error = "ERROR";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Online, Offline, Maintenance, Error
    };

    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Offline : value.Trim().ToUpperInvariant();
}
