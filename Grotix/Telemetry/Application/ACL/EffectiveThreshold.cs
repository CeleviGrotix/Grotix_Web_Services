namespace GrotixBackend.Telemetry.Application.ACL;

public sealed record EffectiveThreshold(
    string SensorType,
    double MinValue,
    double MaxValue,
    string Source);
