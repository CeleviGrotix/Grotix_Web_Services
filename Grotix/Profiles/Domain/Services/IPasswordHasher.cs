namespace GrotixBackend.Profiles.Domain.Services;

/// <summary>Servicio de dominio del informe Profile (implementación en infraestructura).</summary>
public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string passwordHash);
}
