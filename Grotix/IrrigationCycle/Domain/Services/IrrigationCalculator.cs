namespace GrotixBackend.IrrigationCycle.Domain.Services;

public static class IrrigationCalculator
{
    private const double DefaultVolumeLiters = 25;
    private const double LitersPerHumidityPoint = 2.5;
    private const int DefaultDurationMinutes = 15;
    private const double FlowLitersPerMinute = 5;

    public static double CalculateVolumeLiters(double? currentHumidity, double? targetHumidity)
    {
        if (!currentHumidity.HasValue || !targetHumidity.HasValue)
            return DefaultVolumeLiters;

        var deficit = Math.Max(0, targetHumidity.Value - currentHumidity.Value);
        if (deficit <= 0)
            return DefaultVolumeLiters;

        return Math.Round(Math.Max(5, deficit * LitersPerHumidityPoint), 2);
    }

    public static int EstimateDurationMinutes(double volumeLiters) =>
        Math.Max(1, (int)Math.Ceiling(volumeLiters / FlowLitersPerMinute));

    public static int ResolveDurationMinutes(double volumeLiters, int? requestedMinutes) =>
        requestedMinutes is > 0 ? requestedMinutes.Value : EstimateDurationMinutes(volumeLiters);
}
