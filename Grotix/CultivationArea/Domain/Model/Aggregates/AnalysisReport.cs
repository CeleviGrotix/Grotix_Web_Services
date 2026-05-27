namespace GrotixBackend.CultivationArea.Domain.Model.Aggregates;

/// <summary>Informe de análisis de cultivo por zona (tabla <c>analysis_report</c>).</summary>
public class AnalysisReport
{
    public int Id { get; private set; }
    public int ZoneId { get; private set; }
    public string DetectedPhase { get; private set; } = null!;
    public float HealthScore { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected AnalysisReport() { }

    public AnalysisReport(int zoneId, string detectedPhase, float healthScore, DateTime? createdAt = null)
    {
        if (zoneId <= 0)
            throw new ArgumentException("ZoneId inválido.");
        if (string.IsNullOrWhiteSpace(detectedPhase))
            throw new ArgumentException("DetectedPhase requerido.");
        if (healthScore is < 0f or > 100f)
            throw new ArgumentException("HealthScore debe estar entre 0 y 100.");

        ZoneId = zoneId;
        DetectedPhase = detectedPhase.Trim();
        HealthScore = healthScore;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}
