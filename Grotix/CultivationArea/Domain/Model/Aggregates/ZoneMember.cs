namespace GrotixBackend.CultivationArea.Domain.Model.Aggregates;

/// <summary>
/// Asignación de un miembro de la organización a una zona (permiso de visibilidad).
/// </summary>
public class ZoneMember
{
    public int Id { get; private set; }
    public int ZoneId { get; private set; }
    public int UserId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public int AssignedByUserId { get; private set; }

    protected ZoneMember() { }

    public ZoneMember(int zoneId, int userId, int assignedByUserId)
    {
        if (zoneId <= 0)
            throw new ArgumentException("ZoneId inválido.");
        if (userId <= 0)
            throw new ArgumentException("UserId inválido.");
        if (assignedByUserId <= 0)
            throw new ArgumentException("AssignedByUserId inválido.");

        ZoneId = zoneId;
        UserId = userId;
        AssignedByUserId = assignedByUserId;
        AssignedAt = DateTime.UtcNow;
    }
}
