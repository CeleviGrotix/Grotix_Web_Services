using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GrotixBackend.CultivationArea.Infrastructure.Reporting;

public sealed class QuestPdfZoneReportRenderer : IZoneReportPdfRenderer
{
    static QuestPdfZoneReportRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Render(ZoneReportData data) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("Grotix — Zone Report").FontSize(20).Bold().FontColor(Colors.Teal.Darken2);
                    column.Item().PaddingTop(4).Text($"Generated: {data.GeneratedAtUtc:yyyy-MM-dd HH:mm} UTC")
                        .FontColor(Colors.Grey.Darken1);
                    column.Item().Text($"Period: {data.PeriodStartUtc:yyyy-MM-dd} → {data.PeriodEndUtc:yyyy-MM-dd}")
                        .FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(14);

                    column.Item().Element(c => Section(c, "Zone", rows =>
                    {
                        rows.Item().Text($"{data.Zone.Name} (ID {data.Zone.Id})");
                        rows.Item().Text($"Crop: {data.Zone.CropName}");
                        rows.Item().Text($"Irrigation: {data.Zone.IrrigationMode}");
                        rows.Item().Text($"Phase: {data.Zone.CurrentPhase ?? "N/A"}");
                        rows.Item().Text($"Coordinates: {data.Zone.Latitude:F4}, {data.Zone.Longitude:F4}");
                    }));

                    column.Item().Element(c => Section(c, "Farm & Association", rows =>
                    {
                        rows.Item().Text($"{data.Farm.Name} (ID {data.Farm.Id})");
                        rows.Item().Text($"Location: {data.Farm.Location}");
                        rows.Item().Text($"Association: {data.Farm.AssociationName} (ID {data.Farm.AssociationId})");
                    }));

                    column.Item().Element(c => Section(c, "Microcontrollers", rows =>
                    {
                        if (data.Devices.Count == 0)
                        {
                            rows.Item().Text("No devices assigned to this zone.");
                            return;
                        }

                        rows.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("ID").Bold();
                                header.Cell().Text("Model").Bold();
                                header.Cell().Text("MAC").Bold();
                                header.Cell().Text("Status").Bold();
                                header.Cell().Text("Sensors").Bold();
                                header.Cell().Text("Actuators").Bold();
                            });

                            foreach (var device in data.Devices)
                            {
                                table.Cell().Text(device.DeviceId.ToString());
                                table.Cell().Text(device.Model);
                                table.Cell().Text(device.MacAddress);
                                table.Cell().Text(device.Status);
                                table.Cell().Text(device.SensorCount.ToString());
                                table.Cell().Text(device.ActuatorCount.ToString());
                            }
                        });
                    }));

                    column.Item().Element(c => Section(c, "Telemetry summary", rows =>
                    {
                        rows.Item().Text($"Readings in period: {data.Telemetry.ReadingsCount}");
                        rows.Item().Text($"Avg temperature: {FormatNullable(data.Telemetry.AvgTemperature, "°C")}");
                        rows.Item().Text($"Avg air humidity: {FormatNullable(data.Telemetry.AvgHumidityAir, "%")}");
                        rows.Item().Text($"Avg soil moisture: {FormatNullable(data.Telemetry.AvgHumiditySoil, "%")}");
                        rows.Item().Text($"Avg light intensity: {FormatNullable(data.Telemetry.AvgLightIntensity, " lux")}");
                    }));

                    column.Item().Element(c => Section(c, "Irrigation summary", rows =>
                    {
                        rows.Item().Text($"Cycles: {data.Irrigation.CyclesCount}");
                        rows.Item().Text($"Total volume: {data.Irrigation.TotalVolumeLiters:F1} L");
                        rows.Item().Text($"Total duration: {data.Irrigation.TotalDurationMinutes} min");
                    }));

                    column.Item().Element(c => Section(c, "Crop analysis reports", rows =>
                    {
                        if (data.AnalysisReports.Count == 0)
                        {
                            rows.Item().Text("No analysis reports in this period.");
                            return;
                        }

                        foreach (var report in data.AnalysisReports)
                        {
                            rows.Item().Text(
                                $"{report.CreatedAt:yyyy-MM-dd HH:mm} — {report.DetectedPhase} — Health {report.HealthScore:F1}%");
                        }
                    }));

                    column.Item().Element(c => Section(c, "Recent alerts", rows =>
                    {
                        if (data.Alerts.Count == 0)
                        {
                            rows.Item().Text("No recent alerts.");
                            return;
                        }

                        foreach (var alert in data.Alerts)
                        {
                            rows.Item().Text($"{alert.CreatedAt:yyyy-MM-dd HH:mm} [{alert.AlertType}] {alert.Message}");
                        }
                    }));
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Grotix IoT Platform — ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();

    private static void Section(IContainer container, string title, Action<ColumnDescriptor> content)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text(title).FontSize(12).Bold().FontColor(Colors.Teal.Darken2);
            column.Item().Column(content);
        });
    }

    private static string FormatNullable(double? value, string suffix) =>
        value.HasValue ? $"{value.Value:F1}{suffix}" : "N/A";
}
