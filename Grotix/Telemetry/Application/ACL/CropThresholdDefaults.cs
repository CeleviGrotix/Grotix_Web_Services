using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.Telemetry.Application.ACL;

public static class CropThresholdDefaults
{
    public static (double Min, double Max)? ForSensorType(Crop crop, string sensorType, string? sensorUnit = null)
    {
        return SensorTypes.Normalize(sensorType) switch
        {
            SensorTypes.AirTemperature => (crop.OptimalTemperature - 5, crop.OptimalTemperature + 5),
            SensorTypes.AirHumidity    => (crop.OptimalHumidityAir - 10, crop.OptimalHumidityAir + 10),
            SensorTypes.SoilMoisture   => (crop.OptimalHumiditySoil - 10, crop.OptimalHumiditySoil + 10),
            SensorTypes.LightIntensity => ResolveLightThresholds(crop.OptimalLight, sensorUnit),
            _ => null
        };
    }

    private static (double Min, double Max) ResolveLightThresholds(double optimalLight, string? sensorUnit)
    {
        if (LightMeasurementScale.IsPercentUnit(sensorUnit))
        {
            var optimal = LightMeasurementScale.ToPercentOptimal(optimalLight);
            return (optimal * 0.7, optimal * 1.3);
        }

        return (optimalLight * 0.7, optimalLight * 1.3);
    }
}
