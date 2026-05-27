namespace GrotixBackend.Contracts.Integration.Irrigation;

/// <summary>Ciclo de riego finalizado (completado o abortado).</summary>
public sealed record IrrigationCompletedIntegrationEvent(
    int CycleId,
    int ZoneId,
    double VolumeLiters,
    string Status,
    DateTime CompletedAt);
