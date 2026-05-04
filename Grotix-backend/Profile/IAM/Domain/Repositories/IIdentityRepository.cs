using GrotixBackend.IAM.Domain.Model.Aggregates;
using System;
using System.Threading.Tasks;

namespace GrotixBackend.IAM.Domain.Repositories
{
    public interface IIdentityRepository
    {
        Task<Identity> GetByIdAsync(int id);
        Task<Identity> GetByUsernameAsync(string username);
        Task AddAsync(Identity identity);
        Task UpdateAsync(Identity identity);

        Task<bool> ExistsByUsernameAsync(string username);
    }
}