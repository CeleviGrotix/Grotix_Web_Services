using GrotixBackend.Telemetry.Domain.Model.Entities;

namespace GrotixBackend.Telemetry.Domain.Services;

public static class ReadingRangeValidator
{
    public static bool IsPhysicallyValid(Sensor sensor, double value)
    {
        if (sensor.MinPhysical.HasValue && value < sensor.MinPhysical.Value)
            return false;
        if (sensor.MaxPhysical.HasValue && value > sensor.MaxPhysical.Value)
            return false;
        return true;
    }
}
