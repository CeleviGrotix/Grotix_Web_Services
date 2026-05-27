namespace GrotixBackend.Telemetry.Domain.Services;

public static class MovingAverageFilter
{
    public static double Smooth(IReadOnlyList<double> recentValues, double newValue, int windowSize = 3)
    {
        if (recentValues.Count == 0)
            return newValue;

        var take = Math.Min(windowSize - 1, recentValues.Count);
        var slice = recentValues.TakeLast(take).Append(newValue);
        return slice.Average();
    }
}
