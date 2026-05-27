namespace GrotixBackend.IrrigationCycle.Domain.Model.ValueObjects;

public static class CycleStatuses
{
    public const string Scheduled = "SCHEDULED";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Aborted = "ABORTED";

    public static string Normalize(string status) =>
        status.Trim().ToUpperInvariant() switch
        {
            "ACTIVE" => InProgress,
            var s when s is Scheduled or InProgress or Completed or Aborted => s,
            _ => throw new ArgumentException($"Estado de ciclo no válido: {status}")
        };

    public static bool IsActive(string status) =>
        Normalize(status) == InProgress;

    public static string ToApiStatus(string status) =>
        Normalize(status) == InProgress ? "ACTIVE" : Normalize(status);
}
