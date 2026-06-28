using FluentAssertions;
using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;
using Xunit;

namespace CultivationArea.Api.Tests.Unit;

/// <summary>
/// US21 — Generación y descarga de reportes históricos
/// </summary>
public class US21_ReportGenerationUnitTests
{
    private static ZoneReportData BuildReportData(
        DateTime? from = null,
        DateTime? to = null,
        int readingsCount = 100,
        int cyclesCount = 5) =>
        new ZoneReportData(
            GeneratedAtUtc: DateTime.UtcNow,
            PeriodStartUtc: from ?? DateTime.UtcNow.AddDays(-30),
            PeriodEndUtc: to ?? DateTime.UtcNow,
            Zone: new ZoneReportZoneInfo(1, "Zona Test", "Tomate", "AUTOMATIC", "Vegetativo", -12.0, -77.0),
            Farm: new ZoneReportFarmInfo(1, "Granja Central", "Lima", 1, "Asociación Test"),
            Devices: new List<ZoneReportDeviceInfo>(),
            Telemetry: new ZoneReportTelemetrySummary(readingsCount, 22.5, 65.0, 45.0, 80.0),
            Irrigation: new ZoneReportIrrigationSummary(cyclesCount, 120.5, 60),
            AnalysisReports: new List<ZoneReportAnalysisInfo>(),
            Alerts: new List<ZoneReportAlertInfo>());

    // Escenario 1 — Rangos de tiempo

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(365)]
    public void ReportData_PeriodStartIsBeforePeriodEnd_ForAllRanges(int days)
    {
        var from = DateTime.UtcNow.AddDays(-days);
        var to = DateTime.UtcNow;
        var report = BuildReportData(from, to);

        report.PeriodStartUtc.Should().BeBefore(report.PeriodEndUtc);
    }

    [Fact]
    public void ReportData_HasGeneratedAtUtc_WhenCreated()
    {
        var report = BuildReportData();
        report.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // Escenario 2 — Métricas clave

    [Fact]
    public void ReportData_TelemetrySummary_HasReadingsCount()
    {
        var report = BuildReportData(readingsCount: 150);
        report.Telemetry.ReadingsCount.Should().Be(150);
    }

    [Fact]
    public void ReportData_TelemetrySummary_HasAverageValues()
    {
        var report = BuildReportData();
        report.Telemetry.AvgTemperature.Should().Be(22.5);
        report.Telemetry.AvgHumidityAir.Should().Be(65.0);
        report.Telemetry.AvgHumiditySoil.Should().Be(45.0);
        report.Telemetry.AvgLightIntensity.Should().Be(80.0);
    }

    [Fact]
    public void ReportData_IrrigationSummary_HasCyclesCount()
    {
        var report = BuildReportData(cyclesCount: 10);
        report.Irrigation.CyclesCount.Should().Be(10);
    }

    [Fact]
    public void ReportData_IrrigationSummary_HasTotalVolume()
    {
        var report = BuildReportData();
        report.Irrigation.TotalVolumeLiters.Should().Be(120.5);
    }

    // Escenario 3 — Exportación

    [Fact]
    public void ReportData_ZoneInfo_HasExpectedFields()
    {
        var report = BuildReportData();
        report.Zone.Name.Should().Be("Zona Test");
        report.Zone.CropName.Should().Be("Tomate");
        report.Zone.IrrigationMode.Should().Be("AUTOMATIC");
    }

    [Fact]
    public void ReportData_FarmInfo_HasExpectedFields()
    {
        var report = BuildReportData();
        report.Farm.Name.Should().Be("Granja Central");
        report.Farm.Location.Should().Be("Lima");
    }

    [Fact]
    public void ReportData_PeriodOf30Days_HasCorrectDuration()
    {
        var from = DateTime.UtcNow.AddDays(-30);
        var to = DateTime.UtcNow;
        var report = BuildReportData(from, to);

        var duration = report.PeriodEndUtc - report.PeriodStartUtc;
        duration.Days.Should().BeCloseTo(30, 1);
    }
}