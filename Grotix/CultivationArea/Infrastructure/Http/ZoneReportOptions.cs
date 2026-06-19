namespace GrotixBackend.CultivationArea.Infrastructure.Http;

public sealed class ZoneReportOptions
{
    public const string SectionName = "ZoneReports";

    /// <summary>Base URL del Gateway (o del host único en local). Sin barra final.</summary>
    public string GatewayBaseUrl { get; set; } = "http://localhost:5100";
}
