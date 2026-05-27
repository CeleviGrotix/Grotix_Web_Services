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

        MicrocontrollerId = microcontrollerId;
        Type = type.Trim().ToUpperInvariant();
        Pin = pin;
    }
}
