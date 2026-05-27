namespace GrotixBackend.Telemetry.Domain.Services;

public static class ThresholdEvaluator
{
    public static bool IsOutOfRange(double value, double min, double max) =>
        value < min || value > max;

    public static double NearestBreachedThreshold(double value, double min, double max) =>
        value < min ? min : max;
}
