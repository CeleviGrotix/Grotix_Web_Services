namespace GrotixBackend.HardwareDevice.Domain.Model.Entities;

/// <summary>Cola de comandos de actuador (tabla <c>action_queue</c>).</summary>
public class ActionQueueItem
{
    public const string StatusPending = "PENDING";
    public const string StatusSent = "SENT";
    public const string StatusCompleted = "COMPLETED";
    public const string StatusFailed = "FAILED";

    public int Id { get; private set; }
    public int ActuatorId { get; private set; }
    public string Command { get; private set; } = null!;
    public string Status { get; private set; } = StatusPending;
    public DateTime CreatedAt { get; private set; }

    protected ActionQueueItem() { }

    public ActionQueueItem(int actuatorId, string command, DateTime? createdAt = null)
    {
        if (actuatorId <= 0)
            throw new ArgumentException("ActuatorId inválido.");
        if (string.IsNullOrWhiteSpace(command))
            throw new ArgumentException("Command requerido.");

        ActuatorId = actuatorId;
        Command = command.Trim().ToUpperInvariant();
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public void MarkSent() => Status = StatusSent;

    public void MarkCompleted() => Status = StatusCompleted;

    public void MarkFailed() => Status = StatusFailed;
}
