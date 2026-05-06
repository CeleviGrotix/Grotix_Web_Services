namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

using GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>Entidad del informe Profile: asociación agraria (contacto Name, Email).</summary>
public class Association
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public UserEmail ContactEmail { get; private set; } = null!;

    protected Association() { }

    public Association(string name, UserEmail contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la asociación no puede estar vacío.");
        Name = name.Trim();
        ContactEmail = contactEmail;
    }

    public void Update(string name, UserEmail contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la asociación no puede estar vacío.");
        Name = name.Trim();
        ContactEmail = contactEmail;
    }
}
