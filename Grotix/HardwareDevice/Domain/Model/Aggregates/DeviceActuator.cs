using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;

namespace GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

/// <summary>Actuador asociado a un microcontrolador (tabla <c>actuator</c>).</summary>
public class DeviceActuator
{
    public int Id { get; private set; }
    public int MicrocontrollerId { get; private set; }
    public string Type { get; private set; } = null!;
    public int Pin { get; private set; }
    public string Status { get; private set; } = "CLOSED";
    public bool CurrentState { get; private set; }
    public DateTime? LastSeen { get; private set; }

    protected DeviceActuator() { }

    public DeviceActuator(int microcontrollerId, string type, int pin)
    {
        if (microcontrollerId <= 0)
            throw new ArgumentException("MicrocontrollerId inválido.");
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type requerido.");

        var normalizedType = ActuatorTypes.Normalize(type);
        if (!ActuatorTypes.IsValid(normalizedType))
            throw new ArgumentException(
                $"Tipo de actuador no válido. Valores permitidos: {ActuatorTypes.AllowedValuesLabel()}.");

        MicrocontrollerId = microcontrollerId;
        Type = normalizedType;
        Pin = pin;
    }

    public void SetOpen()
    {
        CurrentState = true;
        Status = "OPEN";
        LastSeen = DateTime.UtcNow;
    }

    public void SetClosed()
    {
        CurrentState = false;
        Status = "CLOSED";
        LastSeen = DateTime.UtcNow;
    }
}
