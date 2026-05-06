namespace GrotixBackend.CultivationArea.Domain.Model.Aggregates;

/// <summary>Granja del agricultor (tabla <c>farm</c>; propietario <c>UserID</c> del perfil).</summary>
public class Farm
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Location { get; private set; } = null!;

    protected Farm() { }

    public Farm(int userId, string name, string location)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId inválido.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la granja no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La ubicación no puede estar vacía.");

        UserId = userId;
        Name = name.Trim();
        Location = location.Trim();
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
