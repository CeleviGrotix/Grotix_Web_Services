using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.Telemetry.Application.ACL;

public static class CropThresholdDefaults
{
    public static (double Min, double Max)? ForSensorType(Crop crop, string sensorType)
    {
        return SensorTypes.Normalize(sensorType) switch
        {
            SensorTypes.AirTemperature => (crop.OptimalTemperature - 5, crop.OptimalTemperature + 5),
            SensorTypes.AirHumidity    => (crop.OptimalHumidityAir - 10, crop.OptimalHumidityAir + 10),
            SensorTypes.SoilMoisture   => (crop.OptimalHumiditySoil - 10, crop.OptimalHumiditySoil + 10),
            SensorTypes.LightIntensity => (crop.OptimalLight * 0.7, crop.OptimalLight * 1.3),
            _ => null
        };
    }
}
