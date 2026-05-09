namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>Códigos persistidos en <c>permission.Code</c> y como claims JWT tipo <c>permission</c>.</summary>
public static class KnownPermissionCodes
{
    public const string TelemetryView = "TELEMETRY_VIEW";
    public const string TelemetryExport = "TELEMETRY_EXPORT";
    public const string AnalysisView = "ANALYSIS_VIEW";
    public const string ManualControlExecute = "MANUAL_CONTROL_EXECUTE";
    public const string ThresholdWrite = "THRESHOLD_WRITE";
    public const string DeviceConfig = "DEVICE_CONFIG";
    public const string UserInvite = "USER_INVITE";
    public const string UserDelete = "USER_DELETE";
    public const string RoleAssign = "ROLE_ASSIGN";
    public const string SystemLogsView = "SYSTEM_LOGS_VIEW";
    public const string HardwareDiagnostic = "HARDWARE_DIAGNOSTIC";
}
