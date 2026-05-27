namespace GrotixBackend.Telemetry.Application.Internal.Alerting;

public sealed class AlertEvaluationOptions
{
    public const string SectionName = "Telemetry:Alerts";

    /// <summary>Lecturas consecutivas fuera de umbral antes de disparar alerta.</summary>
    public int SustainedReadingsBeforeAlert { get; set; } = 4;
}
