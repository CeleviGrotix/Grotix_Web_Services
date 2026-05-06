using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

/// <summary>Contrato del informe Profile; implementación Core DB: <c>CoreDbUserRepository</c>.</summary>
public interface IUserRepository : IAsyncRepository<User>
{
    Task<User?> GetByIdentityIdAsync(int identityId);
}