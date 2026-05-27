namespace GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

/// <summary>Ciclo de riego (tabla <c>irrigation_cycle</c>).</summary>
public class IrrigationCycleRecord
{
    public int Id { get; private set; }
    public int ZoneId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public double VolumeLiters { get; private set; }
    public int DurationMinutes { get; private set; }
    public string Status { get; private set; } = null!;
    public string? AbortReason { get; private set; }

    protected IrrigationCycleRecord() { }

    public IrrigationCycleRecord(int zoneId, double volumeLiters, int durationMinutes)
    {
        if (zoneId <= 0)
            throw new ArgumentException("ZoneId inválido.");
        if (volumeLiters <= 0)
            throw new ArgumentException("VolumeLiters debe ser positivo.");
        if (durationMinutes <= 0)
            throw new ArgumentException("DurationMinutes debe ser positivo.");

        ZoneId = zoneId;
        VolumeLiters = volumeLiters;
        DurationMinutes = durationMinutes;
        StartTime = DateTime.UtcNow;
        Status = ValueObjects.CycleStatuses.InProgress;
    }

    public DateTime PlannedEndUtc() => StartTime.AddMinutes(DurationMinutes);

    public void Complete(double? actualVolumeLiters = null)
    {
        Status = ValueObjects.CycleStatuses.Completed;
        EndTime = DateTime.UtcNow;
        if (actualVolumeLiters.HasValue && actualVolumeLiters.Value > 0)
            VolumeLiters = actualVolumeLiters.Value;
    }

    public void Abort(string reason)
    {
        Status = ValueObjects.CycleStatuses.Aborted;
        EndTime = DateTime.UtcNow;
        AbortReason = string.IsNullOrWhiteSpace(reason) ? "ABORTED" : reason.Trim();
    }
}
