namespace GrotixBackend.Telemetry.Application.ACL;

/// <summary>
/// LDR / firmware suelen enviar 0–100 (%); BH1750 envía lux.
/// La unidad viene del catálogo de sensor (tabla sensor, campo Unit).
/// </summary>
public static class LightMeasurementScale
{
    public static bool IsPercentUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return false;

        var normalized = unit.Trim().TrimEnd('.').ToLowerInvariant();
        return normalized is "%" or "percent" or "pct" or "percentage";
    }

    /// <summary>
    /// Convierte optimalLight del cultivo a escala 0–100 cuando el sensor usa %.
    /// Valores &lt;= 100 se asumen ya en %. Valores mayores (p. ej. 800 en BD) se
    /// interpretan como lux de referencia → 80% (800/1000*100).
    /// </summary>
    public static double ToPercentOptimal(double optimalLight) =>
        optimalLight <= 100
            ? optimalLight
            : Math.Clamp(optimalLight / 1000.0 * 100.0, 0, 100);
}
