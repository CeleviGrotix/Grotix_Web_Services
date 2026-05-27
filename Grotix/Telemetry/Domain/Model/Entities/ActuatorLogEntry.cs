namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>Historial de acciones de actuador (tabla <c>actuator_log</c>).</summary>
public class ActuatorLogEntry
{
    public long Id { get; private set; }
    public int ActuatorId { get; private set; }
    public string Action { get; private set; } = null!;
    public int? Duration { get; private set; }
    public DateTime Timestamp { get; private set; }
    public float? FlowRate { get; private set; }

    protected ActuatorLogEntry() { }

    public ActuatorLogEntry(
        int actuatorId,
        string action,
        DateTime? timestamp = null,
        int? duration = null,
        float? flowRate = null)
    {
        if (actuatorId <= 0)
            throw new ArgumentException("ActuatorId inválido.");
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action requerido.");

        ActuatorId = actuatorId;
        Action = action.Trim().ToUpperInvariant();
        Timestamp = timestamp ?? DateTime.UtcNow;
        Duration = duration;
        FlowRate = flowRate;
    }
}
