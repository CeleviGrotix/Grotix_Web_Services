namespace GrotixBackend.Contracts.Integration.Irrigation;

/// <summary>Ciclo de riego iniciado.</summary>
public sealed record IrrigationStartedIntegrationEvent(
    int CycleId,
    int ZoneId,
    double VolumeLiters,
    int DurationMinutes,
    DateTime StartedAt);
