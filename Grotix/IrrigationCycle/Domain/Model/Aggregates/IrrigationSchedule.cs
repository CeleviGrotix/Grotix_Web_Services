namespace GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;

/// <summary>Programa de riego (tabla <c>irrigation_schedule</c>).</summary>
public class IrrigationSchedule
{
    public int Id { get; private set; }
    public int ZoneId { get; private set; }
    public string DaysOfTheWeek { get; private set; } = null!;
    public TimeOnly StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected IrrigationSchedule() { }

    public IrrigationSchedule(
        int zoneId,
        string daysOfTheWeek,
        TimeOnly startTime,
        int durationMinutes,
        bool isActive = true)
    {
        if (zoneId <= 0)
            throw new ArgumentException("ZoneId inválido.");
        if (string.IsNullOrWhiteSpace(daysOfTheWeek))
            throw new ArgumentException("DaysOfTheWeek requerido.");
        if (durationMinutes <= 0)
            throw new ArgumentException("DurationMinutes debe ser positivo.");

        ZoneId = zoneId;
        DaysOfTheWeek = NormalizeDays(daysOfTheWeek);
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string? daysOfTheWeek,
        TimeOnly? startTime,
        int? durationMinutes,
        bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(daysOfTheWeek))
            DaysOfTheWeek = NormalizeDays(daysOfTheWeek);
        if (startTime.HasValue)
            StartTime = startTime.Value;
        if (durationMinutes.HasValue)
        {
            if (durationMinutes.Value <= 0)
                throw new ArgumentException("DurationMinutes debe ser positivo.");
            DurationMinutes = durationMinutes.Value;
        }

        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public void ToggleStatus() => IsActive = !IsActive;

    private static string NormalizeDays(string days) =>
        string.Join(',', days.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => d.Trim().ToUpperInvariant()));
}
