using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.Telemetry.Application.Internal;

public sealed class TelemetryIngestService(
    ISensorRepository sensorRepository,
    ISensorReadingRepository sensorReadingRepository,
    IAlertEvaluationService alertEvaluationService) : ITelemetryIngestService
{
    public async Task IngestAsync(TelemetryReceivedIntegrationEvent evt, CancellationToken cancellationToken = default)
    {
        var reading = new SensorReading
        {
            DeviceId = evt.DeviceId,
            ZoneId = evt.ZoneId,
            Temperature = evt.Temperature,
            HumidityAir = evt.HumidityAir,
            HumiditySoil = evt.HumiditySoil,
            LightIntensity = evt.LightIntensity,
            Timestamp = evt.Timestamp.ToUniversalTime()
        };

        await sensorReadingRepository.AddAsync(reading, cancellationToken);

        var sensors = await sensorRepository.ListByZoneAsync(evt.ZoneId, cancellationToken);
        foreach (var sensor in sensors)
        {
            var value = ResolveFieldForSensor(sensor.Type, evt);
            if (value is null) continue;
            await alertEvaluationService.EvaluateAsync(sensor, value.Value, reading.Timestamp, cancellationToken);
        }
    }

    private static double? ResolveFieldForSensor(string sensorType, TelemetryReceivedIntegrationEvent evt) =>
        sensorType.ToUpperInvariant() switch
        {
            SensorTypes.AirTemperature => evt.Temperature,
            SensorTypes.AirHumidity    => evt.HumidityAir,
            SensorTypes.SoilMoisture   => evt.HumiditySoil,
            SensorTypes.LightIntensity => evt.LightIntensity,
            _ => null
        };
}
