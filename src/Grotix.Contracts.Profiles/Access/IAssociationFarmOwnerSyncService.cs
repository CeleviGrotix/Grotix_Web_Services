namespace GrotixBackend.Contracts.Profiles.Access;

/// <summary>
/// Asigna el <c>user_admin</c> de la asociación como dueño (<c>UserID</c>) de granjas sin propietario.
/// </summary>
public interface IAssociationFarmOwnerSyncService
{
    Task SyncUnownedFarmsAsync(int associationId, CancellationToken cancellationToken = default);
}
