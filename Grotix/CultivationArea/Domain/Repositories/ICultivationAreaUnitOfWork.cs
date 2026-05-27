namespace GrotixBackend.CultivationArea.Domain.Repositories;

/// <summary>
/// Unidad de trabajo específica de CultivationArea para persistir entidades de cultivo en su propio contexto.
/// </summary>
public interface ICultivationAreaUnitOfWork
{
    Task CompleteAsync();
}
