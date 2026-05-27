namespace GrotixBackend.IAM.Domain.Repositories;

/// <summary>
/// Unidad de trabajo específica de IAM para persistir identidades en su propio contexto.
/// </summary>
public interface IIamUnitOfWork
{
    Task CompleteAsync();
}
