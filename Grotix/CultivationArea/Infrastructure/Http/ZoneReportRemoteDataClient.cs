using System.Net.Http.Json;
using System.Text.Json;
using GrotixBackend.CultivationArea.Application.ACL;
using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.CultivationArea.Infrastructure.Http;

public sealed class ZoneReportRemoteDataClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    IOptions<ZoneReportOptions> options,
    ILogger<ZoneReportRemoteDataClient> logger) : IZoneReportRemoteDataClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string?> GetAssociationNameAsync(
        int associationId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/associations/{associationId}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var payload = await response.Content.ReadFromJsonAsync<AssociationDto>(JsonOptions, cancellationToken);
        return payload?.Name;
    }

    public async Task<IReadOnlyList<ZoneReportDeviceInfo>> GetZoneDevicesAsync(
        int zoneId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/hardware/zones/{zoneId}/devices",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Zone report: devices for zone {ZoneId} returned {Status}", zoneId, response.StatusCode);
            return [];
        }

        var items = await response.Content.ReadFromJsonAsync<List<DeviceDto>>(JsonOptions, cancellationToken)
            ?? [];

        return items.Select(d => new ZoneReportDeviceInfo(
            d.DeviceId != 0 ? d.DeviceId : d.Id,
            d.Model ?? "MCU",
            d.MacAddress ?? "-",
            d.Status ?? "UNKNOWN",
            d.SensorCount,
            d.ActuatorCount,
            d.LastSeen)).ToList();
    }

    public async Task<ZoneReportTelemetrySummary> GetTelemetrySummaryAsync(
        int zoneId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"?startTime={Uri.EscapeDataString(startUtc.ToString("O"))}" +
            $"&endTime={Uri.EscapeDataString(endUtc.ToString("O"))}&limit=5000";

        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/telemetry/zones/{zoneId}{query}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Zone report: telemetry for zone {ZoneId} returned {Status}", zoneId, response.StatusCode);
            return EmptyTelemetry();
        }

        var payload = await response.Content.ReadFromJsonAsync<TelemetryHistoryDto>(JsonOptions, cancellationToken);
        var readings = payload?.Readings ?? [];

        if (readings.Count == 0)
            return EmptyTelemetry();

        return new ZoneReportTelemetrySummary(
            readings.Count,
            Average(readings, r => r.Temperature),
            Average(readings, r => r.HumidityAir),
            Average(readings, r => r.HumiditySoil),
            Average(readings, r => r.LightIntensity));
    }

    public async Task<ZoneReportIrrigationSummary> GetIrrigationSummaryAsync(
        int zoneId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"?startTime={Uri.EscapeDataString(startUtc.ToString("O"))}" +
            $"&endTime={Uri.EscapeDataString(endUtc.ToString("O"))}&limit=500";

        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/irrigation/history/{zoneId}{query}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Zone report: irrigation for zone {ZoneId} returned {Status}", zoneId, response.StatusCode);
            return new ZoneReportIrrigationSummary(0, 0, 0);
        }

        var cycles = await response.Content.ReadFromJsonAsync<List<IrrigationCycleDto>>(JsonOptions, cancellationToken)
            ?? [];

        return new ZoneReportIrrigationSummary(
            cycles.Count,
            cycles.Sum(c => c.VolumeLiters ?? 0),
            cycles.Sum(c => c.DurationMinutes ?? 0));
    }

    public async Task<IReadOnlyList<ZoneReportAlertInfo>> GetAlertsAsync(
        int zoneId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/telemetry/zones/{zoneId}/alerts?limit={Math.Clamp(limit, 1, 50)}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return [];

        var alerts = await response.Content.ReadFromJsonAsync<List<AlertDto>>(JsonOptions, cancellationToken)
            ?? [];

        return alerts.Select(a =>
        {
            var message =
                $"Value {a.Value:F2} ({a.BreachDirection ?? "?"}) — threshold {a.BreachedThreshold:F2}";
            return new ZoneReportAlertInfo(
                a.SensorType ?? "SENSOR",
                message,
                a.TriggeredAt ?? DateTime.UtcNow);
        }).ToList();
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.GatewayBaseUrl.TrimEnd('/');
        using var request = new HttpRequestMessage(method, $"{baseUrl}{relativePath}");

        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization))
            request.Headers.TryAddWithoutValidation("Authorization", authorization);

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static ZoneReportTelemetrySummary EmptyTelemetry() =>
        new(0, null, null, null, null);

    private static double? Average(IReadOnlyList<TelemetryReadingDto> readings, Func<TelemetryReadingDto, double> selector)
    {
        if (readings.Count == 0)
            return null;

        return readings.Average(selector);
    }

    private sealed class AssociationDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class DeviceDto
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string? Model { get; set; }
        public string? MacAddress { get; set; }
        public string? Status { get; set; }
        public int SensorCount { get; set; }
        public int ActuatorCount { get; set; }
        public DateTime? LastSeen { get; set; }
    }

    private sealed class TelemetryHistoryDto
    {
        public List<TelemetryReadingDto>? Readings { get; set; }
    }

    private sealed class TelemetryReadingDto
    {
        public double Temperature { get; set; }
        public double HumidityAir { get; set; }
        public double HumiditySoil { get; set; }
        public double LightIntensity { get; set; }
    }

    private sealed class IrrigationCycleDto
    {
        public double? VolumeLiters { get; set; }
        public int? DurationMinutes { get; set; }
    }

    private sealed class AlertDto
    {
        public string? SensorType { get; set; }
        public double Value { get; set; }
        public string? BreachDirection { get; set; }
        public double BreachedThreshold { get; set; }
        public DateTime? TriggeredAt { get; set; }
    }
}
