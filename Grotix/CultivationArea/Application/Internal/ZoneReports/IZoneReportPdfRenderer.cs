using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;

namespace GrotixBackend.CultivationArea.Application.Internal.ZoneReports;

public interface IZoneReportPdfRenderer
{
    byte[] Render(ZoneReportData data);
}
