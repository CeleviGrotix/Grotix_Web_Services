namespace GrotixBackend.CultivationArea.Application.Internal.ZoneReports;

public sealed record ZoneReportData(
    DateTime GeneratedAtUtc,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    ZoneReportZoneInfo Zone,
    ZoneReportFarmInfo Farm,
    IReadOnlyList<ZoneReportDeviceInfo> Devices,
    ZoneReportTelemetrySummary Telemetry,
    ZoneReportIrrigationSummary Irrigation,
    IReadOnlyList<ZoneReportAnalysisInfo> AnalysisReports,
    IReadOnlyList<ZoneReportAlertInfo> Alerts);

public sealed record ZoneReportZoneInfo(
    int Id,
    string Name,
    string CropName,
    string IrrigationMode,
    string? CurrentPhase,
    double Latitude,
    double Longitude);

public sealed record ZoneReportFarmInfo(
    int Id,
    string Name,
    string Location,
    int AssociationId,
    string AssociationName);

public sealed record ZoneReportDeviceInfo(
    int DeviceId,
    string Model,
    string MacAddress,
    string Status,
    int SensorCount,
    int ActuatorCount,
    DateTime? LastSeen);

public sealed record ZoneReportTelemetrySummary(
    int ReadingsCount,
    double? AvgTemperature,
    double? AvgHumidityAir,
    double? AvgHumiditySoil,
    double? AvgLightIntensity);

public sealed record ZoneReportIrrigationSummary(
    int CyclesCount,
    double TotalVolumeLiters,
    int TotalDurationMinutes);

public sealed record ZoneReportAnalysisInfo(
    string DetectedPhase,
    float HealthScore,
    DateTime CreatedAt);

public sealed record ZoneReportAlertInfo(
    string AlertType,
    string Message,
    DateTime CreatedAt);
