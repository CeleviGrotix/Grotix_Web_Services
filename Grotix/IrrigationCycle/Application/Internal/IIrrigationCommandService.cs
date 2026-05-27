using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public interface IIrrigationCommandService
{
    Task<IrrigationCycleRecord> StartManualAsync(
        int zoneId,
        double? volumeLiters,
        int? durationMinutes,
        CancellationToken cancellationToken = default);

    Task CompleteCycleAsync(int cycleId, CancellationToken cancellationToken = default);

    /// <summary>Cancela el ciclo IN_PROGRESS de la zona (cierre manual del actuador).</summary>
    Task<IrrigationCycleRecord> AbortActiveByZoneAsync(
        int zoneId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}
