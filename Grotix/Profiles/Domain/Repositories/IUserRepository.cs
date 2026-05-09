using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

/// <summary>Contrato del informe Profile; implementación Core DB: <c>CoreDbUserRepository</c>.</summary>
public interface IUserRepository : IAsyncRepository<User>
{
    Task<User?> GetByIdentityIdAsync(int identityId);

    /// <summary>Solo roles agricultor (user_admin, user_basic, user_advanced).</summary>
    Task<IReadOnlyList<User>> ListFarmersOrderedByIdAsync(CancellationToken cancellationToken = default);

    /// <summary>Agricultores de la asociación.</summary>
    Task<IReadOnlyList<User>> ListFarmersByAssociationIdAsync(int associationId, CancellationToken cancellationToken = default);

    /// <summary>Usuario por id solo si es agricultor; si no existe o es admin/staff, <c>null</c>.</summary>
    Task<User?> GetFarmerByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Indica si la asociación ya tiene al menos un usuario con rol <c>user_admin</c> (3).</summary>
    Task<bool> HasUserAdminForAssociationAsync(int associationId, CancellationToken cancellationToken = default);
}