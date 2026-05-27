using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using GrotixBackend.IrrigationCycle.Domain.Services;

namespace GrotixBackend.IrrigationCycle.Application.Internal;

public sealed class IrrigationCommandService(
    IIrrigationCycleRepository cycleRepository,
    IIrrigationUnitOfWork unitOfWork,
    IZoneAccessService zoneAccessService,
    IIrrigationContextService irrigationContextService,
    IActuatorControlService actuatorControlService,
    IIrrigationCompletedPublisher completedPublisher) : IIrrigationCommandService
{
    public async Task<IrrigationCycleRecord> StartManualAsync(
        int zoneId,
        double? volumeLiters,
        int? durationMinutes,
        CancellationToken cancellationToken = default)
    {
        if (!await zoneAccessService.ZoneExistsAsync(zoneId))
            throw new ArgumentException("La zona no existe.");

        var active = await cycleRepository.GetActiveByZoneAsync(zoneId);
        if (active != null)
            throw new InvalidOperationException($"Ya hay un ciclo activo en la zona {zoneId}.");

        var context = await irrigationContextService.GetZoneContextAsync(zoneId, cancellationToken);
        var volume = volumeLiters ?? IrrigationCalculator.CalculateVolumeLiters(
            context?.CurrentHumidityPercent,
            context?.OptimalHumidity);
        var duration = IrrigationCalculator.ResolveDurationMinutes(volume, durationMinutes);

        var cycle = new IrrigationCycleRecord(zoneId, volume, duration);
        await cycleRepository.AddAsync(cycle);
        await unitOfWork.CompleteAsync(cancellationToken);

        await actuatorControlService.TryActivateIrrigationAsync(zoneId, cancellationToken);
        return cycle;
    }

    public async Task CompleteCycleAsync(int cycleId, CancellationToken cancellationToken = default)
    {
        var cycle = await cycleRepository.GetByIdAsync(cycleId)
            ?? throw new KeyNotFoundException($"Ciclo {cycleId} no encontrado.");

        if (!Domain.Model.ValueObjects.CycleStatuses.IsActive(cycle.Status))
            return;

        cycle.Complete();
        await actuatorControlService.TryDeactivateIrrigationAsync(cycle.ZoneId, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        completedPublisher.Publish(cycle);
    }

    public async Task<IrrigationCycleRecord> AbortActiveByZoneAsync(
        int zoneId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (!await zoneAccessService.ZoneExistsAsync(zoneId))
            throw new ArgumentException("La zona no existe.");

        var active = await cycleRepository.GetActiveByZoneAsync(zoneId);
        if (active == null)
            throw new InvalidOperationException($"No hay un ciclo activo en la zona {zoneId}.");

        active.Abort(reason ?? "MANUAL_CANCEL");
        await actuatorControlService.TryDeactivateIrrigationAsync(zoneId, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        completedPublisher.Publish(active);
        return active;
    }
}
