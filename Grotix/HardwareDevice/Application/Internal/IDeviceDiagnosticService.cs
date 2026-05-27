namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IDeviceDiagnosticService
{
    Task<DeviceDiagnosticReport?> RunAsync(int deviceId, bool includeSensors, bool includeActuators);
}

public sealed record DeviceDiagnosticReport(
    int DeviceId,
    DateTime Timestamp,
    string OverallStatus,
    IReadOnlyDictionary<string, object> Checks);
