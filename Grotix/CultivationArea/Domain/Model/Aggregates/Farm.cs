namespace GrotixBackend.CultivationArea.Domain.Model.Aggregates;

/// <summary>Granja vinculada a una asociación; el dueño es el <c>user_admin</c> de la organización.</summary>
public class Farm
{
    public int Id { get; private set; }
    public int? UserId { get; private set; }
    public int AssociationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Location { get; private set; } = null!;

    protected Farm() { }

    public Farm(int? userId, int associationId, string name, string location)
    {
        if (associationId <= 0)
            throw new ArgumentException("AssociationId inválido.");
        if (userId is <= 0)
            throw new ArgumentException("UserId inválido.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la granja no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La ubicación no puede estar vacía.");

        UserId = userId;
        AssociationId = associationId;
        Name = name.Trim();
        Location = location.Trim();
    }

    public void AssignOwner(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId inválido.");
        UserId = userId;
    }

    public void Update(string name, string location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la granja no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La ubicación no puede estar vacía.");
        Name = name.Trim();
        Location = location.Trim();
    }
}
