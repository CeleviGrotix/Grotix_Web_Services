namespace GrotixBackend.Profiles.Domain.Repositories;

/// <summary>
/// Unidad de trabajo específica de Profiles para persistir sus agregados en el contexto dedicado.
/// </summary>
public interface IProfilesUnitOfWork
{
    Task CompleteAsync();
}
