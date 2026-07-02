namespace GrotixBackend.IrrigationCycle.Domain.Services;

/// <summary>
/// Provides helper methods to calculate irrigation volume and duration.
/// </summary>
public static class IrrigationCalculator
{
    // Default irrigation volume used when humidity data is unavailable.
    private const double DefaultVolumeLiters = 25;

    // Estimated liters required to increase soil humidity by one percentage point.
    private const double LitersPerHumidityPoint = 2.5;

    // Default irrigation duration in minutes.
    private const int DefaultDurationMinutes = 15;

    // Estimated irrigation system flow rate (liters per minute).
    private const double FlowLitersPerMinute = 5;

    public static double CalculateVolumeLiters(double? currentHumidity, double? targetHumidity)
    {
        // Fallback to a default volume when humidity information is missing.
        if (!currentHumidity.HasValue || !targetHumidity.HasValue)
            return DefaultVolumeLiters;

        var deficit = Math.Max(0, targetHumidity.Value - currentHumidity.Value);

        // No additional irrigation is needed when the target has already been reached.
        if (deficit <= 0)
            return DefaultVolumeLiters;

        return Math.Round(Math.Max(5, deficit * LitersPerHumidityPoint), 2);
    }

    // Estimates irrigation time based on the configured flow rate.
    public static int EstimateDurationMinutes(double volumeLiters) =>
        Math.Max(1, (int)Math.Ceiling(volumeLiters / FlowLitersPerMinute));

    // Uses the requested duration when provided; otherwise estimates it automatically.
    public static int ResolveDurationMinutes(double volumeLiters, int? requestedMinutes) =>
        requestedMinutes is > 0 ? requestedMinutes.Value : EstimateDurationMinutes(volumeLiters);
}
